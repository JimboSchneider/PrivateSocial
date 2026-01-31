using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.ApiService.Data;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.ApiService.Services;
using PrivateSocial.Tests.Helpers;

namespace PrivateSocial.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _configuration = TestConfigurationBuilder.CreateJwtConfiguration();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_context, _configuration, _loggerMock.Object);
        
        // Reset test data builder for each test class instance
        TestDataBuilder.Reset();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var password = "ValidPass123!";
        var firstName = "Test";
        var lastName = "User";

        // Act
        var result = await _authService.RegisterAsync(username, email, password, firstName, lastName);

        // Assert
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be(username);
        result.User.Email.Should().Be(email);
        result.User.FirstName.Should().Be(firstName);
        result.User.LastName.Should().Be(lastName);
        result.Error.Should().BeEmpty();

        // Verify user was saved to database
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken: TestContext.Current.CancellationToken);
        savedUser.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify(password, savedUser!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldReturnError()
    {
        // Arrange
        var existingUser = TestDataBuilder.CreateUser();
        await SeedUser(existingUser);

        // Act
        var result = await _authService.RegisterAsync(
            existingUser.Username, 
            "newemail@example.com", 
            "Password123!");

        // Assert
        result.Success.Should().BeFalse();
        result.User.Should().BeNull();
        result.Error.Should().Be("Username already exists");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnError()
    {
        // Arrange
        var existingUser = TestDataBuilder.CreateUser();
        await SeedUser(existingUser);

        // Act
        var result = await _authService.RegisterAsync(
            "newuser", 
            existingUser.Email, 
            "Password123!");

        // Assert
        result.Success.Should().BeFalse();
        result.User.Should().BeNull();
        result.Error.Should().Be("Email already registered");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var password = "ValidPass123!";
        var user = TestDataBuilder.CreateUser(password: password);
        await SeedUser(user);
        
        // Get the saved user with generated Id
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);

        // Act
        var result = await _authService.LoginAsync(user.Username, password);

        // Assert
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.Error.Should().BeEmpty();

        // Verify token is valid
        VerifyTokenClaims(result.Token, savedUser!);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ShouldReturnError()
    {
        // Act
        var result = await _authService.LoginAsync("nonexistent", "ValidPass123!");

        // Assert
        AssertLoginFailed(result);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(password: "CorrectPassword!");
        await SeedUser(user);

        // Act
        var result = await _authService.LoginAsync(user.Username, "WrongPassword!");

        // Assert
        AssertLoginFailed(result);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnError()
    {
        // Arrange
        var password = "ValidPass123!";
        var user = TestDataBuilder.CreateUser(password: password, isActive: false);
        await SeedUser(user);

        // Act
        var result = await _authService.LoginAsync(user.Username, password);

        // Assert
        AssertLoginFailed(result);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WithExistingActiveUser_ShouldReturnUser()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        await SeedUser(user);

        // Act
        var result = await _authService.GetUserByUsernameAsync(user.Username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(isActive: false);
        await SeedUser(user);

        // Act
        var result = await _authService.GetUserByUsernameAsync(user.Username);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GenerateJwtToken_ShouldCreateValidToken()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        user.Id = 1; // Set an ID for the token generation test

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        VerifyTokenClaims(token, user);
    }

    // Helper methods
    private async Task SeedUser(User user)
    {
        // Let EF Core generate the ID
        user.Id = 0;
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    private void AssertLoginFailed((bool Success, string Token, string Error) result)
    {
        result.Success.Should().BeFalse();
        result.Token.Should().BeEmpty();
        result.Error.Should().Be("Invalid username or password");
    }

    private void VerifyTokenClaims(string token, User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);
        
        jwt.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == user.Username);
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
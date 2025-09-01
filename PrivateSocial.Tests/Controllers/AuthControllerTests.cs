using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.ApiService.Controllers;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.ApiService.Models.Auth;
using PrivateSocial.ApiService.Services;
using PrivateSocial.Tests.Helpers;
using System.Security.Claims;

namespace PrivateSocial.Tests.Controllers;

public class AuthControllerTests : ControllerTestBase
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_loggerMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var user = new User
        {
            Id = 1,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        _authServiceMock.Setup(x => x.RegisterAsync(
                request.Username, 
                request.Email, 
                request.Password, 
                request.FirstName, 
                request.LastName))
            .ReturnsAsync((true, user, string.Empty));

        _authServiceMock.Setup(x => x.GenerateJwtToken(user))
            .Returns("test-jwt-token");

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<AuthResponse>().Subject;
        
        response.Token.Should().Be("test-jwt-token");
        response.Username.Should().Be(request.Username);
        response.Email.Should().Be(request.Email);
        response.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));

        _authServiceMock.Verify(x => x.RegisterAsync(
            request.Username, 
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName), Times.Once);
    }

    [Fact]
    public async Task Register_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        _authServiceMock.Setup(x => x.RegisterAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string?>(), 
                It.IsAny<string?>()))
            .ReturnsAsync((false, null, "Username already exists"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value;
        response.Should().BeEquivalentTo(new { message = "Username already exists" });
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Test123!"
        };

        var user = new User
        {
            Id = 1,
            Username = request.Username,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = "Test",
            LastName = "User"
        };

        _authServiceMock.Setup(x => x.LoginAsync(request.Username, request.Password))
            .ReturnsAsync((true, "test-jwt-token", string.Empty));

        _authServiceMock.Setup(x => x.GetUserByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<AuthResponse>().Subject;
        
        response.Token.Should().Be("test-jwt-token");
        response.Username.Should().Be(request.Username);
        response.Email.Should().Be(user.Email);
        response.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));

        _authServiceMock.Verify(x => x.LoginAsync(request.Username, request.Password), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword"
        };

        _authServiceMock.Setup(x => x.LoginAsync(request.Username, request.Password))
            .ReturnsAsync((false, string.Empty, "Invalid username or password"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var response = unauthorizedResult.Value;
        response.Should().BeEquivalentTo(new { message = "Invalid username or password" });
    }

    [Fact]
    public async Task Login_WhenUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Test123!"
        };

        _authServiceMock.Setup(x => x.LoginAsync(request.Username, request.Password))
            .ReturnsAsync((true, "test-jwt-token", string.Empty));

        _authServiceMock.Setup(x => x.GetUserByUsernameAsync(request.Username))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var response = unauthorizedResult.Value;
        response.Should().BeEquivalentTo(new { message = "User not found" });
    }

    [Fact]
    public async Task GetCurrentUser_WithAuthenticatedUser_ShouldReturnUserInfo()
    {
        // Arrange
        var username = "testuser";
        var user = new User
        {
            Id = 1,
            Username = username,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Test",
            LastName = "User",
            Bio = "Test bio",
            ProfilePictureUrl = "https://example.com/pic.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _controller.ControllerContext = CreateControllerContext(user.Id, username);
        _authServiceMock.Setup(x => x.GetUserByUsernameAsync(username))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userDto = okResult.Value.Should().BeAssignableTo<UserDto>().Subject;
        
        userDto.Id.Should().Be(user.Id);
        userDto.Username.Should().Be(user.Username);
        userDto.Email.Should().Be(user.Email);
        userDto.FirstName.Should().Be(user.FirstName);
        userDto.LastName.Should().Be(user.LastName);
        userDto.Bio.Should().Be(user.Bio);
        userDto.ProfilePictureUrl.Should().Be(user.ProfilePictureUrl);
        userDto.IsActive.Should().Be(user.IsActive);
        userDto.CreatedAt.Should().Be(user.CreatedAt);
    }

    [Fact]
    public async Task GetCurrentUser_WithNoAuthenticatedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext();

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var username = "testuser";
        _controller.ControllerContext = CreateControllerContext(1, username);
        _authServiceMock.Setup(x => x.GetUserByUsernameAsync(username))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetCurrentUser_WithEmptyUsername_ShouldReturnUnauthorized(string? username)
    {
        // Arrange
        var claims = new List<Claim>();
        if (username != null)
        {
            claims.Add(new Claim(ClaimTypes.Name, username));
        }
        
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
}
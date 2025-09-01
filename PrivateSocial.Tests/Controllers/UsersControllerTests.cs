using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.ApiService.Controllers;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.Tests.Helpers;

namespace PrivateSocial.Tests.Controllers;

public class UsersControllerTests : ControllerTestBase
{
    private readonly UsersController _controller;
    private readonly Mock<ILogger<UsersController>> _loggerMock;

    public UsersControllerTests()
    {
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_loggerMock.Object, Context);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            TestDataBuilder.CreateUser(username: "user1", email: "user1@example.com"),
            TestDataBuilder.CreateUser(username: "user2", email: "user2@example.com"),
            TestDataBuilder.CreateUser(username: "user3", email: "user3@example.com")
        };

        foreach (var user in users)
        {
            await SeedUserAsync(user);
        }

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        
        returnedUsers.Should().HaveCount(3);
        returnedUsers.Should().Contain(u => u.Username == "user1");
        returnedUsers.Should().Contain(u => u.Username == "user2");
        returnedUsers.Should().Contain(u => u.Username == "user3");
    }

    [Fact]
    public async Task GetUsers_WithNoUsers_ShouldReturnEmptyList()
    {
        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        
        returnedUsers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUser_WithExistingId_ShouldReturnUser()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(
            username: "testuser",
            email: "test@example.com",
            firstName: "Test",
            lastName: "User"
        );
        user.Bio = "Test bio";
        user.ProfilePictureUrl = "https://example.com/pic.jpg";
        
        await SeedUserAsync(user);

        // Act
        var result = await _controller.GetUser(user.Id);

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
    public async Task GetUser_WithNonExistingId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetUser(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Test123!",
            FirstName = "New",
            LastName = "User",
            Bio = "New user bio"
        };

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var userDto = createdResult.Value.Should().BeAssignableTo<UserDto>().Subject;
        
        userDto.Username.Should().Be(createUserDto.Username);
        userDto.Email.Should().Be(createUserDto.Email);
        userDto.FirstName.Should().Be(createUserDto.FirstName);
        userDto.LastName.Should().Be(createUserDto.LastName);
        userDto.Bio.Should().Be(createUserDto.Bio);
        userDto.IsActive.Should().BeTrue();

        createdResult.ActionName.Should().Be(nameof(UsersController.GetUser));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(userDto.Id);

        // Verify user was saved to database
        var savedUser = await Context.Users.FirstOrDefaultAsync(u => u.Username == createUserDto.Username, TestContext.Current.CancellationToken);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be(createUserDto.Email);
        BCrypt.Net.BCrypt.Verify(createUserDto.Password, savedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_WithExistingUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var existingUser = TestDataBuilder.CreateUser(username: "existinguser", email: "existing@example.com");
        await SeedUserAsync(existingUser);

        var createUserDto = new CreateUserDto
        {
            Username = "existinguser", // Same username
            Email = "newemail@example.com",
            Password = "Test123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Username already exists");
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var existingUser = TestDataBuilder.CreateUser(username: "existinguser", email: "existing@example.com");
        await SeedUserAsync(existingUser);

        var createUserDto = new CreateUserDto
        {
            Username = "newuser",
            Email = "existing@example.com", // Same email
            Password = "Test123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Email already exists");
    }

    [Fact]
    public async Task CreateUser_WithMinimalData_ShouldCreateUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Username = "minimaluser",
            Email = "minimal@example.com",
            Password = "Test123!"
            // No FirstName, LastName, or Bio
        };

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var userDto = createdResult.Value.Should().BeAssignableTo<UserDto>().Subject;
        
        userDto.Username.Should().Be(createUserDto.Username);
        userDto.Email.Should().Be(createUserDto.Email);
        userDto.FirstName.Should().BeNull();
        userDto.LastName.Should().BeNull();
        userDto.Bio.Should().BeNull();
        userDto.ProfilePictureUrl.Should().BeNull();
        userDto.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnUsersWithAllProperties()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(
            username: "fulluser",
            email: "full@example.com",
            firstName: "Full",
            lastName: "User"
        );
        user.Bio = "Complete bio";
        user.ProfilePictureUrl = "https://example.com/profile.jpg";
        user.IsActive = false;
        user.CreatedAt = DateTime.UtcNow.AddDays(-5);
        
        await SeedUserAsync(user);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        
        var userDto = returnedUsers.Should().ContainSingle().Subject;
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
}
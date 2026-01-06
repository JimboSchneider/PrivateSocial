using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.ApiService.Controllers;
using PrivateSocial.ApiService.Models;
using PrivateSocial.ApiService.Services;

namespace PrivateSocial.Tests.Controllers;

public class PostsControllerTests : ControllerTestBase
{
    private readonly PostsController _controller;
    private readonly Mock<IPostService> _postServiceMock;
    private readonly Mock<ILogger<PostsController>> _loggerMock;

    public PostsControllerTests()
    {
        _postServiceMock = new Mock<IPostService>();
        _loggerMock = new Mock<ILogger<PostsController>>();
        _controller = new PostsController(_postServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnPagedResults()
    {
        // Arrange
        var pagedResult = new PagedResult<PostDto>
        {
            Items = new List<PostDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            TotalPages = 0
        };

        _postServiceMock.Setup(x => x.GetPostsAsync(1, 10))
            .ReturnsAsync(pagedResult);
        
        // Act
        var result = await _controller.GetPosts(page: 1, pageSize: 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(pagedResult);
    }

    [Fact]
    public async Task GetPost_WithExistingId_ShouldReturnPost()
    {
        // Arrange
        var postDto = new PostDto
        {
            Id = 1,
            Content = "Test Content",
            AuthorId = 1,
            AuthorName = "TestUser",
            CreatedAt = DateTime.UtcNow
        };

        _postServiceMock.Setup(x => x.GetPostByIdAsync(1))
            .ReturnsAsync(postDto);

        // Act
        var result = await _controller.GetPost(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(postDto);
    }

    [Fact]
    public async Task GetPost_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        _postServiceMock.Setup(x => x.GetPostByIdAsync(999))
            .ReturnsAsync((PostDto?)null);

        // Act
        var result = await _controller.GetPost(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreatePost_WithValidData_ShouldCreatePost()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext(1, "TestUser");
        
        var request = new CreatePostRequest { Content = "New post content" };
        var createdPost = new PostDto
        {
            Id = 1,
            Content = request.Content,
            AuthorId = 1,
            AuthorName = "TestUser",
            CreatedAt = DateTime.UtcNow
        };

        _postServiceMock.Setup(x => x.CreatePostAsync(request, 1))
            .ReturnsAsync(createdPost);

        // Act
        var result = await _controller.CreatePost(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(createdPost);
    }

    [Fact]
    public async Task CreatePost_WithMissingUserClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext(); // No user ID
        var request = new CreatePostRequest { Content = "New post content" };

        // Act
        var result = await _controller.CreatePost(request);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.Value.Should().Be("User ID claim is missing.");
    }

    [Fact]
    public async Task UpdatePost_AsPostOwner_ShouldUpdatePost()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext(1);
        var request = new UpdatePostRequest { Content = "Updated content" };
        var updatedPost = new PostDto
        {
            Id = 1,
            Content = request.Content,
            AuthorId = 1,
            AuthorName = "TestUser",
            CreatedAt = DateTime.UtcNow
        };

        _postServiceMock.Setup(x => x.UpdatePostAsync(1, request, 1))
            .ReturnsAsync(updatedPost);

        // Act
        var result = await _controller.UpdatePost(1, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(updatedPost);
    }

    [Fact]
    public async Task UpdatePost_AsNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext(2); // Different user
        var request = new UpdatePostRequest { Content = "Updated content" };

        _postServiceMock.Setup(x => x.UpdatePostAsync(1, request, 2))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.UpdatePost(1, request);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task UpdatePost_WithNonExistingPost_ShouldReturnNotFound()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext(1);
        var request = new UpdatePostRequest { Content = "Updated content" };

        _postServiceMock.Setup(x => x.UpdatePostAsync(999, request, 1))
            .ReturnsAsync((PostDto?)null);

        // Act
        var result = await _controller.UpdatePost(999, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeletePost_AsPostOwner_ShouldDeletePost()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext(1);

        _postServiceMock.Setup(x => x.DeletePostAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePost(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeletePost_AsNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext(2);

        _postServiceMock.Setup(x => x.DeletePostAsync(1, 2))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.DeletePost(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeletePost_WithNonExistingPost_ShouldReturnNotFound()
    {
        // Arrange
        _controller.ControllerContext = CreateControllerContext(1);

        _postServiceMock.Setup(x => x.DeletePostAsync(999, 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePost(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
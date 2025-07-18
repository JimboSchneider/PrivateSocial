using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateSocial.ApiService.Controllers;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.Tests.Helpers;

namespace PrivateSocial.Tests.Controllers;

public class PostsControllerTests : ControllerTestBase
{
    private readonly PostsController _controller;

    public PostsControllerTests()
    {
        _controller = new PostsController(Context);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnPagedResults()
    {
        // Arrange
        await SeedMultiplePostsAsync(15);
        
        // Act
        var result = await _controller.GetPosts(page: 1, pageSize: 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeAssignableTo<PagedResult<PostDto>>().Subject;
        
        pagedResult.Items.Should().HaveCount(10);
        pagedResult.TotalCount.Should().Be(15);
        pagedResult.Page.Should().Be(1);
        pagedResult.PageSize.Should().Be(10);
        pagedResult.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetPosts_WithUserIdFilter_ShouldReturnUserPosts()
    {
        // Arrange
        var userId = 1;
        await SeedMultiplePostsAsync(5, userId: userId);
        await SeedMultiplePostsAsync(3, userId: 2);

        // Act
        var result = await _controller.GetPosts(userId: userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeAssignableTo<PagedResult<PostDto>>().Subject;
        
        pagedResult.Items.Should().HaveCount(5);
        pagedResult.Items.Should().OnlyContain(p => p.AuthorId == userId);
    }

    [Fact]
    public async Task GetPost_WithExistingId_ShouldReturnPost()
    {
        // Arrange
        var post = await SeedPostAsync();

        // Act
        var result = await _controller.GetPost(post.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var postDto = okResult.Value.Should().BeAssignableTo<PostDto>().Subject;
        
        postDto.Id.Should().Be(post.Id);
        postDto.Content.Should().Be(post.Content);
        postDto.AuthorId.Should().Be(post.UserId);
    }

    [Fact]
    public async Task GetPost_WithNonExistingId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetPost(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreatePost_WithValidData_ShouldCreatePost()
    {
        // Arrange
        var user = await SeedUserAsync();
        _controller.ControllerContext = CreateControllerContext(user.Id, user.Username);
        
        var request = new CreatePostRequest { Content = "New post content" };

        // Act
        var result = await _controller.CreatePost(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var postDto = createdResult.Value.Should().BeAssignableTo<PostDto>().Subject;
        
        postDto.Content.Should().Be(request.Content);
        postDto.AuthorId.Should().Be(user.Id);
        postDto.AuthorName.Should().Be(user.Username);

        // Verify post was saved
        var savedPost = await Context.Posts.FirstOrDefaultAsync(p => p.Id == postDto.Id);
        savedPost.Should().NotBeNull();
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
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.Value.Should().Be("User ID claim is missing.");
    }

    [Fact]
    public async Task UpdatePost_AsPostOwner_ShouldUpdatePost()
    {
        // Arrange
        var user = await SeedUserAsync();
        var post = await SeedPostAsync(TestDataBuilder.CreatePost(userId: user.Id));
        _controller.ControllerContext = CreateControllerContext(user.Id);
        
        var request = new UpdatePostRequest { Content = "Updated content" };

        // Act
        var result = await _controller.UpdatePost(post.Id, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var postDto = okResult.Value.Should().BeAssignableTo<PostDto>().Subject;
        
        postDto.Content.Should().Be(request.Content);

        // Verify post was updated
        var updatedPost = await Context.Posts.FindAsync(post.Id);
        updatedPost!.Content.Should().Be(request.Content);
    }

    [Fact]
    public async Task UpdatePost_AsNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        var postOwner = await SeedUserAsync();
        var otherUser = await SeedUserAsync();
        var post = await SeedPostAsync(TestDataBuilder.CreatePost(userId: postOwner.Id));
        
        _controller.ControllerContext = CreateControllerContext(otherUser.Id);
        var request = new UpdatePostRequest { Content = "Updated content" };

        // Act
        var result = await _controller.UpdatePost(post.Id, request);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task UpdatePost_WithNonExistingPost_ShouldReturnNotFound()
    {
        // Arrange
        var user = await SeedUserAsync();
        _controller.ControllerContext = CreateControllerContext(user.Id);
        var request = new UpdatePostRequest { Content = "Updated content" };

        // Act
        var result = await _controller.UpdatePost(999, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeletePost_AsPostOwner_ShouldDeletePost()
    {
        // Arrange
        var user = await SeedUserAsync();
        var post = await SeedPostAsync(TestDataBuilder.CreatePost(userId: user.Id));
        _controller.ControllerContext = CreateControllerContext(user.Id);

        // Act
        var result = await _controller.DeletePost(post.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify post was deleted
        var deletedPost = await Context.Posts.FindAsync(post.Id);
        deletedPost.Should().BeNull();
    }

    [Fact]
    public async Task DeletePost_AsNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        var postOwner = await SeedUserAsync();
        var otherUser = await SeedUserAsync();
        var post = await SeedPostAsync(TestDataBuilder.CreatePost(userId: postOwner.Id));
        
        _controller.ControllerContext = CreateControllerContext(otherUser.Id);

        // Act
        var result = await _controller.DeletePost(post.Id);

        // Assert
        result.Should().BeOfType<ForbidResult>();

        // Verify post was not deleted
        var existingPost = await Context.Posts.FindAsync(post.Id);
        existingPost.Should().NotBeNull();
    }

    [Fact]
    public async Task DeletePost_WithNonExistingPost_ShouldReturnNotFound()
    {
        // Arrange
        var user = await SeedUserAsync();
        _controller.ControllerContext = CreateControllerContext(user.Id);

        // Act
        var result = await _controller.DeletePost(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("invalid", "")]
    public async Task CreatePost_WithInvalidUserClaim_ShouldReturnBadRequest(string? claimValue, string expectedError)
    {
        // Arrange
        if (claimValue != null)
        {
            _controller.ControllerContext = CreateControllerContext();
            _controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, claimValue)
                })
            );
        }
        else
        {
            _controller.ControllerContext = CreateControllerContext();
        }
        
        var request = new CreatePostRequest { Content = "New post content" };

        // Act
        var result = await _controller.CreatePost(request);

        // Assert
        if (claimValue == null)
        {
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }
        else
        {
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid User ID claim.");
        }
    }
}
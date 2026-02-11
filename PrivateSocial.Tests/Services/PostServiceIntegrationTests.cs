using FluentAssertions;
using MassTransit;
using Moq;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.ApiService.Models;
using PrivateSocial.ApiService.Services;
using PrivateSocial.Contracts.Events;

namespace PrivateSocial.Tests.Services;

public class PostServiceIntegrationTests : IntegrationTestBase
{
    private PostService _postService = null!;
    private Mock<IPublishEndpoint> _publishEndpointMock = null!;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _postService = new PostService(Context, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task CreatePost_ShouldPersistToDatabase()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var request = new CreatePostRequest { Content = "Integration Test Content" };

        // Act
        var result = await _postService.CreatePostAsync(request, user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Content.Should().Be(request.Content);

        var savedPost = await Context.Posts.FindAsync(result.Id);
        savedPost.Should().NotBeNull();
        savedPost!.Content.Should().Be(request.Content);

        // Verify PostCreated event was published
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<PostCreated>(e =>
                    e.PostId == result.Id &&
                    e.UserId == user.Id &&
                    e.Content == request.Content),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnPostsFromDatabase()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            Context.Posts.Add(new Post { Content = $"Post {i}", UserId = user.Id });
        }
        await Context.SaveChangesAsync();

        // Act
        var result = await _postService.GetPostsAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
    }
    [Fact]
    public async Task UpdatePost_AsOwner_ShouldUpdateContent()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var post = new Post { Content = "Original Content", UserId = user.Id };
        Context.Posts.Add(post);
        await Context.SaveChangesAsync();

        var request = new UpdatePostRequest { Content = "Updated Content" };

        // Act
        var result = await _postService.UpdatePostAsync(post.Id, request, user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("Updated Content");

        var updatedPost = await Context.Posts.FindAsync(post.Id);
        updatedPost!.Content.Should().Be("Updated Content");

        // Verify PostUpdated event was published
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<PostUpdated>(e =>
                    e.PostId == post.Id &&
                    e.UserId == user.Id &&
                    e.Content == "Updated Content"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdatePost_AsNonOwner_ShouldThrowUnauthorized()
    {
        // Arrange
        var owner = new User { Username = "owner", Email = "owner@example.com", PasswordHash = "hash" };
        var other = new User { Username = "other", Email = "other@example.com", PasswordHash = "hash" };
        Context.Users.AddRange(owner, other);
        await Context.SaveChangesAsync();

        var post = new Post { Content = "Original Content", UserId = owner.Id };
        Context.Posts.Add(post);
        await Context.SaveChangesAsync();

        var request = new UpdatePostRequest { Content = "Updated Content" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _postService.UpdatePostAsync(post.Id, request, other.Id));
    }

    [Fact]
    public async Task DeletePost_AsOwner_ShouldRemovePost()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var post = new Post { Content = "To Delete", UserId = user.Id };
        Context.Posts.Add(post);
        await Context.SaveChangesAsync();

        // Act
        var result = await _postService.DeletePostAsync(post.Id, user.Id);

        // Assert
        result.Should().BeTrue();
        var deletedPost = await Context.Posts.FindAsync(post.Id);
        deletedPost.Should().BeNull();

        // Verify PostDeleted event was published
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<PostDeleted>(e =>
                    e.PostId == post.Id &&
                    e.UserId == user.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeletePost_AsNonOwner_ShouldThrowUnauthorized()
    {
        // Arrange
        var owner = new User { Username = "owner", Email = "owner@example.com", PasswordHash = "hash" };
        var other = new User { Username = "other", Email = "other@example.com", PasswordHash = "hash" };
        Context.Users.AddRange(owner, other);
        await Context.SaveChangesAsync();

        var post = new Post { Content = "To Delete", UserId = owner.Id };
        Context.Posts.Add(post);
        await Context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _postService.DeletePostAsync(post.Id, other.Id));
    }
}

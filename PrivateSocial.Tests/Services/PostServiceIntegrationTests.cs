using FluentAssertions;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.ApiService.Models;
using PrivateSocial.ApiService.Services;

namespace PrivateSocial.Tests.Services;

public class PostServiceIntegrationTests : IntegrationTestBase
{
    private PostService _postService = null!;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _postService = new PostService(Context);
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

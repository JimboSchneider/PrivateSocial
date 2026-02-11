using MassTransit;
using Microsoft.EntityFrameworkCore;
using PrivateSocial.ApiService.Data;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.ApiService.Models;
using PrivateSocial.ApiService.Extensions;
using PrivateSocial.Contracts.Events;

namespace PrivateSocial.ApiService.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public PostService(ApplicationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<PagedResult<PostDto>> GetPostsAsync(int page, int pageSize)
    {
        var query = _context.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                AuthorId = p.UserId,
                AuthorName = p.User.Username,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

        return await query.ToPagedResultAsync(page, pageSize);
    }

    public async Task<PostDto?> GetPostByIdAsync(int id)
    {
        return await _context.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.Id == id)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                AuthorId = p.UserId,
                AuthorName = p.User.Username,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PostDto> CreatePostAsync(CreatePostRequest request, int userId)
    {
        var post = new Post
        {
            Content = request.Content,
            UserId = userId
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new PostCreated
        {
            CorrelationId = Guid.NewGuid(),
            PostId = post.Id,
            UserId = post.UserId,
            Content = post.Content,
            CreatedAt = post.CreatedAt
        });

        // Reload with user information
        await _context.Entry(post)
            .Reference(p => p.User)
            .LoadAsync();

        return new PostDto
        {
            Id = post.Id,
            Content = post.Content,
            AuthorId = post.UserId,
            AuthorName = post.User.Username,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    public async Task<PostDto?> UpdatePostAsync(int id, UpdatePostRequest request, int userId)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return null;
        }

        if (post.UserId != userId)
        {
            throw new UnauthorizedAccessException("User is not the owner of the post");
        }

        post.Content = request.Content;
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new PostUpdated
        {
            CorrelationId = Guid.NewGuid(),
            PostId = post.Id,
            UserId = post.UserId,
            Content = post.Content,
            UpdatedAt = post.UpdatedAt ?? DateTime.UtcNow
        });

        return new PostDto
        {
            Id = post.Id,
            Content = post.Content,
            AuthorId = post.UserId,
            AuthorName = post.User.Username,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    public async Task<bool> DeletePostAsync(int id, int userId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return false;
        }

        if (post.UserId != userId)
        {
            throw new UnauthorizedAccessException("User is not the owner of the post");
        }

        var postId = post.Id;
        var postUserId = post.UserId;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new PostDeleted
        {
            CorrelationId = Guid.NewGuid(),
            PostId = postId,
            UserId = postUserId,
            DeletedAt = DateTime.UtcNow
        });

        return true;
    }

}

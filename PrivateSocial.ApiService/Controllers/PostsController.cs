using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateSocial.ApiService.Data;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.ApiService.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PrivateSocial.ApiService.Controllers;

/// <summary>
/// Handles post-related operations for registered users
/// </summary>
[Authorize]
[Route("api/posts")]
public class PostsController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PostsController> _logger;

    public PostsController(ApplicationDbContext context, ILogger<PostsController> logger)
        : base(logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all posts with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of posts per page (default: 20)</param>
    /// <returns>List of posts with author information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PostDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Limit max page size

        var query = _context.Posts
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                AuthorId = p.UserId,
                AuthorName = p.User.Username,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Ok(new PagedResult<PostDto>
        {
            Items = posts,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    /// <summary>
    /// Gets a specific post by ID
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <returns>The requested post</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPost(int id)
    {
        var post = await _context.Posts
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

        if (post == null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    /// <summary>
    /// Creates a new post
    /// </summary>
    /// <param name="request">Post creation request</param>
    /// <returns>The created post</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("User ID claim is missing.");
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid User ID claim.");
        }
        
        var post = new Post
        {
            Content = request.Content,
            UserId = userId
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Reload with user information
        await _context.Entry(post)
            .Reference(p => p.User)
            .LoadAsync();

        var postDto = new PostDto
        {
            Id = post.Id,
            Content = post.Content,
            AuthorId = post.UserId,
            AuthorName = post.User.Username,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };

        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, postDto);
    }

    /// <summary>
    /// Updates an existing post
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <param name="request">Update request</param>
    /// <returns>The updated post</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("User ID claim is missing.");
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid User ID claim.");
        }
        
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        if (post.UserId != userId)
        {
            return Forbid();
        }

        post.Content = request.Content;
        await _context.SaveChangesAsync();

        var postDto = new PostDto
        {
            Id = post.Id,
            Content = post.Content,
            AuthorId = post.UserId,
            AuthorName = post.User.Username,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };

        return Ok(postDto);
    }

    /// <summary>
    /// Deletes a post
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("User ID claim is missing.");
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid User ID claim.");
        }
        
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        if (post.UserId != userId)
        {
            return Forbid();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreatePostRequest
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Post content must be between 1 and 500 characters")]
    public required string Content { get; set; }
}

public class UpdatePostRequest
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Post content must be between 1 and 500 characters")]
    public required string Content { get; set; }
}

public class PostDto
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public int AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PagedResult<T>
{
    public required List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
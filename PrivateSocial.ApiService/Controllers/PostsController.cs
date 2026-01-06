using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateSocial.ApiService.Models;
using PrivateSocial.ApiService.Services;
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
    private readonly IPostService _postService;

    public PostsController(IPostService postService, ILogger<PostsController> logger)
        : base(logger)
    {
        _postService = postService;
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
        var result = await _postService.GetPostsAsync(page, pageSize);
        return Ok(result);
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
        var post = await _postService.GetPostByIdAsync(id);

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
        
        var postDto = await _postService.CreatePostAsync(request, userId);

        return CreatedAtAction(nameof(GetPost), new { id = postDto.Id }, postDto);
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
        
        try
        {
            var postDto = await _postService.UpdatePostAsync(id, request, userId);
            
            if (postDto == null)
            {
                return NotFound();
            }
            
            return Ok(postDto);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
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
        
        try
        {
            var result = await _postService.DeletePostAsync(id, userId);
            
            if (!result)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

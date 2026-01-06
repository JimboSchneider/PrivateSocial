using System.ComponentModel.DataAnnotations;

namespace PrivateSocial.ApiService.Models;

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

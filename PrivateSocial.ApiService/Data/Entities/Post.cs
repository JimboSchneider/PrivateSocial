namespace PrivateSocial.ApiService.Data.Entities;

public class Post
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}
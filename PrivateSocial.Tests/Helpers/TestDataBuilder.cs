using PrivateSocial.ApiService.Data.Entities;

namespace PrivateSocial.Tests.Helpers;

public class TestDataBuilder
{
    private static int _userIdCounter = 1;
    private static int _postIdCounter = 1;

    public static User CreateUser(
        string? username = null,
        string? email = null,
        string? password = null,
        string? firstName = null,
        string? lastName = null,
        bool isActive = true)
    {
        var id = _userIdCounter++;
        return new User
        {
            // Don't set Id - let EF Core generate it in most cases
            Username = username ?? $"testuser{id}",
            Email = email ?? $"test{id}@example.com",
            PasswordHash = password != null 
                ? BCrypt.Net.BCrypt.HashPassword(password) 
                : BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = firstName ?? "Test",
            LastName = lastName ?? "User",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Post CreatePost(
        int? userId = null,
        string? content = null)
    {
        var id = _postIdCounter++;
        return new Post
        {
            // Don't set Id - let EF Core generate it
            UserId = userId ?? 1,
            Content = content ?? $"Test post content {id}",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void Reset()
    {
        _userIdCounter = 1;
        _postIdCounter = 1;
    }
}
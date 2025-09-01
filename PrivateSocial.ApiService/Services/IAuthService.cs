using PrivateSocial.ApiService.Data.Entities;

namespace PrivateSocial.ApiService.Services;

public interface IAuthService
{
    Task<(bool Success, string Token, string Error)> LoginAsync(string username, string password);
    Task<(bool Success, User? User, string Error)> RegisterAsync(string username, string email, string password, string? firstName = null, string? lastName = null);
    Task<User?> GetUserByUsernameAsync(string username);
    string GenerateJwtToken(User user);
}
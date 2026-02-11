using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrivateSocial.ApiService.Data;
using PrivateSocial.ApiService.Data.Entities;
using PrivateSocial.Contracts.Events;

namespace PrivateSocial.ApiService.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<(bool Success, string Token, string Error)> LoginAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
            {
                return (false, string.Empty, "Invalid username or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (false, string.Empty, "Invalid username or password");
            }

            var token = GenerateJwtToken(user);
            return (true, token, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", username);
            return (false, string.Empty, "An error occurred during login");
        }
    }

    public async Task<(bool Success, User? User, string Error)> RegisterAsync(string username, string email, string password, string? firstName = null, string? lastName = null)
    {
        try
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return (false, null, "Username already exists");
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return (false, null, "Email already registered");
            }

            // Create new user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _publishEndpoint.Publish(new UserRegistered
            {
                CorrelationId = Guid.NewGuid(),
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                RegisteredAt = user.CreatedAt
            });

            return (true, user, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", username);
            return (false, null, "An error occurred during registration");
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        
        // Try to get the secret from configuration first (for local development)
        // If not found, try to get it from the secret name (which will be loaded from Key Vault)
        var secret = jwtSettings["Secret"];
        if (string.IsNullOrEmpty(secret))
        {
            var secretName = jwtSettings["SecretName"];
            if (!string.IsNullOrEmpty(secretName))
            {
                // When Key Vault is configured, the secret will be available directly in configuration
                // using the secret name as the key
                secret = _configuration[secretName];
            }
        }
        
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("JWT Secret not configured");
        }
        
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
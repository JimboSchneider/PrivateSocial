using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateSocial.ApiService.Models.Auth;
using PrivateSocial.ApiService.Services;

namespace PrivateSocial.ApiService.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger, IAuthService authService) 
        : base(logger)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Authentication token and user info</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        Logger.LogInformation("Registration attempt for username: {Username}", request.Username);

        var (success, user, error) = await _authService.RegisterAsync(
            request.Username, 
            request.Email, 
            request.Password,
            request.FirstName,
            request.LastName);

        if (!success || user == null)
        {
            Logger.LogWarning("Registration failed for username: {Username}. Error: {Error}", 
                request.Username, error);
            return BadRequest(new { message = error });
        }

        var token = _authService.GenerateJwtToken(user);
        
        Logger.LogInformation("User registered successfully: {Username}", user.Username);

        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication token and user info</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        Logger.LogInformation("Login attempt for username: {Username}", request.Username);

        var (success, token, error) = await _authService.LoginAsync(request.Username, request.Password);

        if (!success)
        {
            Logger.LogWarning("Login failed for username: {Username}. Error: {Error}", 
                request.Username, error);
            return Unauthorized(new { message = error });
        }

        var user = await _authService.GetUserByUsernameAsync(request.Username);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        Logger.LogInformation("User logged in successfully: {Username}", user.Username);

        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }
}
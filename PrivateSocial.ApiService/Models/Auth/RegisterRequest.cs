using System.ComponentModel.DataAnnotations;
using PrivateSocial.ApiService.Validation;

namespace PrivateSocial.ApiService.Models.Auth;

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100)]
    [StrongPassword]
    public required string Password { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }
}
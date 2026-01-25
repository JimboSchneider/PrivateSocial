using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PrivateSocial.ApiService.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public partial class StrongPasswordAttribute : ValidationAttribute
{
    private const int MinLength = 12;
    
    public StrongPasswordAttribute() 
        : base("Password must be at least 12 characters and contain uppercase, lowercase, number, and special character.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password)
        {
            return new ValidationResult("Password is required.");
        }

        if (password.Length < MinLength)
        {
            return new ValidationResult($"Password must be at least {MinLength} characters long.");
        }

        if (!HasUpperCase().IsMatch(password))
        {
            return new ValidationResult("Password must contain at least one uppercase letter.");
        }

        if (!HasLowerCase().IsMatch(password))
        {
            return new ValidationResult("Password must contain at least one lowercase letter.");
        }

        if (!HasDigit().IsMatch(password))
        {
            return new ValidationResult("Password must contain at least one number.");
        }

        if (!HasSpecialChar().IsMatch(password))
        {
            return new ValidationResult("Password must contain at least one special character (@$!%*?&).");
        }

        return ValidationResult.Success;
    }

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex HasUpperCase();
    
    [GeneratedRegex(@"[a-z]")]
    private static partial Regex HasLowerCase();
    
    [GeneratedRegex(@"\d")]
    private static partial Regex HasDigit();
    
    [GeneratedRegex(@"[@$!%*?&]")]
    private static partial Regex HasSpecialChar();
}

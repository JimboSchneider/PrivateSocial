namespace PrivateSocial.Contracts.Commands;

public record SendWelcomeEmail
{
    public Guid CorrelationId { get; init; }
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

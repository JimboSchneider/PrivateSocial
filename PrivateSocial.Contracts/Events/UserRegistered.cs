namespace PrivateSocial.Contracts.Events;

public record UserRegistered
{
    public Guid CorrelationId { get; init; }
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}

namespace PrivateSocial.Contracts.Events;

public record UserOnboardingCompleted
{
    public Guid CorrelationId { get; init; }
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
}

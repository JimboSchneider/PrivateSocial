namespace PrivateSocial.Contracts.Events;

public record WelcomeEmailSent
{
    public Guid CorrelationId { get; init; }
    public int UserId { get; init; }
    public DateTime SentAt { get; init; }
}

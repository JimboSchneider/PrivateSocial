namespace PrivateSocial.Contracts.Events;

public record PostCreated
{
    public Guid CorrelationId { get; init; }
    public int PostId { get; init; }
    public int UserId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

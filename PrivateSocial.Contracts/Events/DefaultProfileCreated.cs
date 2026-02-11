namespace PrivateSocial.Contracts.Events;

public record DefaultProfileCreated
{
    public Guid CorrelationId { get; init; }
    public int UserId { get; init; }
    public DateTime CreatedAt { get; init; }
}

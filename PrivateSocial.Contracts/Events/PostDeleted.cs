namespace PrivateSocial.Contracts.Events;

public record PostDeleted
{
    public Guid CorrelationId { get; init; }
    public int PostId { get; init; }
    public int UserId { get; init; }
    public DateTime DeletedAt { get; init; }
}

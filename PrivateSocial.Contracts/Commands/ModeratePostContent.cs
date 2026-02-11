namespace PrivateSocial.Contracts.Commands;

public record ModeratePostContent
{
    public Guid CorrelationId { get; init; }
    public int PostId { get; init; }
    public int UserId { get; init; }
    public string Content { get; init; } = string.Empty;
}

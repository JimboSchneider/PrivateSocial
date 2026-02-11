using MassTransit;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Contracts.Events;

namespace PrivateSocial.Worker.Consumers;

public class PostCreatedConsumer : IConsumer<PostCreated>
{
    private readonly ILogger<PostCreatedConsumer> _logger;

    public PostCreatedConsumer(ILogger<PostCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostCreated> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Post created: PostId={PostId} by UserId={UserId}. Sending for moderation. CorrelationId: {CorrelationId}",
            message.PostId,
            message.UserId,
            message.CorrelationId);

        await context.Send(new Uri("queue:moderate-post-content"), new ModeratePostContent
        {
            CorrelationId = message.CorrelationId,
            PostId = message.PostId,
            UserId = message.UserId,
            Content = message.Content
        });
    }
}

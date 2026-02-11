using MassTransit;
using PrivateSocial.Contracts.Commands;

namespace PrivateSocial.Worker.Consumers;

public class ModeratePostContentConsumer : IConsumer<ModeratePostContent>
{
    private readonly ILogger<ModeratePostContentConsumer> _logger;

    public ModeratePostContentConsumer(ILogger<ModeratePostContentConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ModeratePostContent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Moderating content for PostId={PostId}. CorrelationId: {CorrelationId}",
            message.PostId,
            message.CorrelationId);

        // Simulate content moderation processing
        await Task.Delay(200, context.CancellationToken);

        _logger.LogInformation(
            "Content moderation completed for PostId={PostId}. Content approved. CorrelationId: {CorrelationId}",
            message.PostId,
            message.CorrelationId);
    }
}

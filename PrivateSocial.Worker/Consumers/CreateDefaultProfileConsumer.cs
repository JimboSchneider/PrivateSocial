using MassTransit;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Contracts.Events;

namespace PrivateSocial.Worker.Consumers;

public class CreateDefaultProfileConsumer : IConsumer<CreateDefaultProfile>
{
    private readonly ILogger<CreateDefaultProfileConsumer> _logger;

    public CreateDefaultProfileConsumer(ILogger<CreateDefaultProfileConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateDefaultProfile> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Creating default profile for UserId={UserId} ({Username}). CorrelationId: {CorrelationId}",
            message.UserId,
            message.Username,
            message.CorrelationId);

        // Simulate profile creation
        await Task.Delay(300, context.CancellationToken);

        _logger.LogInformation(
            "Default profile created for UserId={UserId}. CorrelationId: {CorrelationId}",
            message.UserId,
            message.CorrelationId);

        await context.Publish(new DefaultProfileCreated
        {
            CorrelationId = message.CorrelationId,
            UserId = message.UserId,
            CreatedAt = DateTime.UtcNow
        });
    }
}

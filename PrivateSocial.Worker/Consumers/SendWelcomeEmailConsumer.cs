using MassTransit;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Contracts.Events;

namespace PrivateSocial.Worker.Consumers;

public class SendWelcomeEmailConsumer : IConsumer<SendWelcomeEmail>
{
    private readonly ILogger<SendWelcomeEmailConsumer> _logger;

    public SendWelcomeEmailConsumer(ILogger<SendWelcomeEmailConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendWelcomeEmail> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Sending welcome email to {Username} ({Email}). CorrelationId: {CorrelationId}",
            message.Username,
            message.Email,
            message.CorrelationId);

        // Simulate email sending delay
        await Task.Delay(500, context.CancellationToken);

        _logger.LogInformation(
            "Welcome email sent to {Email}. CorrelationId: {CorrelationId}",
            message.Email,
            message.CorrelationId);

        await context.Publish(new WelcomeEmailSent
        {
            CorrelationId = message.CorrelationId,
            UserId = message.UserId,
            SentAt = DateTime.UtcNow
        });
    }
}

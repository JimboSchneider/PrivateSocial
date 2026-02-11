using MassTransit;
using PrivateSocial.Contracts.Events;

namespace PrivateSocial.ApiService.Consumers;

public class UserRegisteredLogConsumer : IConsumer<UserRegistered>
{
    private readonly ILogger<UserRegisteredLogConsumer> _logger;

    public UserRegisteredLogConsumer(ILogger<UserRegisteredLogConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegistered> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "User registered: {Username} (Id: {UserId}, Email: {Email}) at {RegisteredAt}. CorrelationId: {CorrelationId}",
            message.Username,
            message.UserId,
            message.Email,
            message.RegisteredAt,
            message.CorrelationId);

        return Task.CompletedTask;
    }
}

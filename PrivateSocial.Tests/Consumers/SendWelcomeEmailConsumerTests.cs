using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Contracts.Events;
using PrivateSocial.Tests.Helpers;
using PrivateSocial.Worker.Consumers;

namespace PrivateSocial.Tests.Consumers;

public class SendWelcomeEmailConsumerTests
{
    private readonly Mock<ILogger<SendWelcomeEmailConsumer>> _loggerMock;
    private readonly SendWelcomeEmailConsumer _consumer;

    public SendWelcomeEmailConsumerTests()
    {
        _loggerMock = new Mock<ILogger<SendWelcomeEmailConsumer>>();
        _consumer = new SendWelcomeEmailConsumer(_loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldPublishWelcomeEmailSentEvent()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new SendWelcomeEmail
        {
            CorrelationId = correlationId,
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com"
        });

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        context.Verify(
            x => x.Publish(
                It.Is<WelcomeEmailSent>(e =>
                    e.CorrelationId == correlationId &&
                    e.UserId == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldLogEmailSending()
    {
        // Arrange
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new SendWelcomeEmail
        {
            CorrelationId = Guid.NewGuid(),
            UserId = 5,
            Username = "emailuser",
            Email = "email@example.com"
        });

        // Act
        await _consumer.Consume(context.Object);

        // Assert — sending + sent
        ConsumerTestHelpers.VerifyLogged(_loggerMock, LogLevel.Information, Times.AtLeast(2));
    }
}

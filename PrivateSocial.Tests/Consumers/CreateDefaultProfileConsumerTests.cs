using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Contracts.Events;
using PrivateSocial.Tests.Helpers;
using PrivateSocial.Worker.Consumers;

namespace PrivateSocial.Tests.Consumers;

public class CreateDefaultProfileConsumerTests
{
    private readonly Mock<ILogger<CreateDefaultProfileConsumer>> _loggerMock;
    private readonly CreateDefaultProfileConsumer _consumer;

    public CreateDefaultProfileConsumerTests()
    {
        _loggerMock = new Mock<ILogger<CreateDefaultProfileConsumer>>();
        _consumer = new CreateDefaultProfileConsumer(_loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldPublishDefaultProfileCreatedEvent()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new CreateDefaultProfile
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
                It.Is<DefaultProfileCreated>(e =>
                    e.CorrelationId == correlationId &&
                    e.UserId == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldLogProfileCreation()
    {
        // Arrange
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new CreateDefaultProfile
        {
            CorrelationId = Guid.NewGuid(),
            UserId = 5,
            Username = "profileuser",
            Email = "profile@example.com"
        });

        // Act
        await _consumer.Consume(context.Object);

        // Assert — start + completion
        ConsumerTestHelpers.VerifyLogged(_loggerMock, LogLevel.Information, Times.AtLeast(2));
    }
}

using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Contracts.Events;
using PrivateSocial.Tests.Helpers;
using PrivateSocial.Worker.Consumers;

namespace PrivateSocial.Tests.Consumers;

public class PostCreatedConsumerTests
{
    private readonly Mock<ILogger<PostCreatedConsumer>> _loggerMock;
    private readonly PostCreatedConsumer _consumer;

    public PostCreatedConsumerTests()
    {
        _loggerMock = new Mock<ILogger<PostCreatedConsumer>>();
        _consumer = new PostCreatedConsumer(_loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldSendModeratePostContentCommand()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var message = new PostCreated
        {
            CorrelationId = correlationId,
            PostId = 10,
            UserId = 1,
            Content = "Hello world!",
            CreatedAt = DateTime.UtcNow
        };

        ModeratePostContent? capturedCommand = null;
        var sendEndpoint = new Mock<ISendEndpoint>();
        sendEndpoint
            .Setup(x => x.Send(It.IsAny<ModeratePostContent>(), It.IsAny<CancellationToken>()))
            .Callback<ModeratePostContent, CancellationToken>((msg, _) => capturedCommand = msg)
            .Returns(Task.CompletedTask);

        var context = ConsumerTestHelpers.CreateMockConsumeContext(message);
        context.Setup(x => x.GetSendEndpoint(It.IsAny<Uri>()))
            .ReturnsAsync(sendEndpoint.Object);

        // Act
        await _consumer.Consume(context.Object);

        // Assert — verify GetSendEndpoint called with correct queue
        context.Verify(
            x => x.GetSendEndpoint(It.Is<Uri>(u => u.ToString() == "queue:moderate-post-content")),
            Times.Once);

        // Assert — verify the command content
        capturedCommand.Should().NotBeNull();
        capturedCommand!.CorrelationId.Should().Be(correlationId);
        capturedCommand.PostId.Should().Be(10);
        capturedCommand.UserId.Should().Be(1);
        capturedCommand.Content.Should().Be("Hello world!");
    }

    [Fact]
    public async Task Consume_ShouldLogPostCreation()
    {
        // Arrange
        var sendEndpoint = new Mock<ISendEndpoint>();
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new PostCreated
        {
            CorrelationId = Guid.NewGuid(),
            PostId = 5,
            UserId = 2,
            Content = "Test content",
            CreatedAt = DateTime.UtcNow
        });
        context.Setup(x => x.GetSendEndpoint(It.IsAny<Uri>()))
            .ReturnsAsync(sendEndpoint.Object);

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        ConsumerTestHelpers.VerifyLogged(_loggerMock, LogLevel.Information, Times.Once());
    }
}

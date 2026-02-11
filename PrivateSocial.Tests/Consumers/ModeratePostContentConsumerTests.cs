using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Tests.Helpers;
using PrivateSocial.Worker.Consumers;

namespace PrivateSocial.Tests.Consumers;

public class ModeratePostContentConsumerTests
{
    private readonly Mock<ILogger<ModeratePostContentConsumer>> _loggerMock;
    private readonly ModeratePostContentConsumer _consumer;

    public ModeratePostContentConsumerTests()
    {
        _loggerMock = new Mock<ILogger<ModeratePostContentConsumer>>();
        _consumer = new ModeratePostContentConsumer(_loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldCompleteSuccessfully()
    {
        // Arrange
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new ModeratePostContent
        {
            CorrelationId = Guid.NewGuid(),
            PostId = 7,
            UserId = 3,
            Content = "Some post content to moderate"
        });

        // Act
        var act = () => _consumer.Consume(context.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldLogModerationStartAndCompletion()
    {
        // Arrange
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new ModeratePostContent
        {
            CorrelationId = Guid.NewGuid(),
            PostId = 3,
            UserId = 1,
            Content = "Content for moderation"
        });

        // Act
        await _consumer.Consume(context.Object);

        // Assert — start + completion
        ConsumerTestHelpers.VerifyLogged(_loggerMock, LogLevel.Information, Times.Exactly(2));
    }
}

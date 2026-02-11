using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSocial.ApiService.Consumers;
using PrivateSocial.Contracts.Events;
using PrivateSocial.Tests.Helpers;

namespace PrivateSocial.Tests.Consumers;

public class UserRegisteredLogConsumerTests
{
    private readonly Mock<ILogger<UserRegisteredLogConsumer>> _loggerMock;
    private readonly UserRegisteredLogConsumer _consumer;

    public UserRegisteredLogConsumerTests()
    {
        _loggerMock = new Mock<ILogger<UserRegisteredLogConsumer>>();
        _consumer = new UserRegisteredLogConsumer(_loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldLogRegistrationEvent()
    {
        // Arrange
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new UserRegistered
        {
            CorrelationId = Guid.NewGuid(),
            UserId = 42,
            Username = "testuser",
            Email = "test@example.com",
            RegisteredAt = DateTime.UtcNow
        });

        // Act
        await _consumer.Consume(context.Object);

        // Assert
        ConsumerTestHelpers.VerifyLoggedContaining(_loggerMock, LogLevel.Information, "testuser");
    }

    [Fact]
    public async Task Consume_ShouldCompleteWithoutThrowing()
    {
        // Arrange
        var context = ConsumerTestHelpers.CreateMockConsumeContext(new UserRegistered
        {
            CorrelationId = Guid.NewGuid(),
            UserId = 1,
            Username = "newuser",
            Email = "new@example.com",
            RegisteredAt = DateTime.UtcNow
        });

        // Act
        var act = () => _consumer.Consume(context.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }
}

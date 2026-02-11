using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace PrivateSocial.Tests.Helpers;

public static class ConsumerTestHelpers
{
    /// <summary>
    /// Creates a mock ConsumeContext with message and CancellationToken configured.
    /// </summary>
    public static Mock<ConsumeContext<T>> CreateMockConsumeContext<T>(T message) where T : class
    {
        var context = new Mock<ConsumeContext<T>>();
        context.Setup(x => x.Message).Returns(message);
        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
        return context;
    }

    /// <summary>
    /// Verifies that the logger was called at the specified level the expected number of times.
    /// </summary>
    public static void VerifyLogged<T>(Mock<ILogger<T>> loggerMock, LogLevel level, Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    /// <summary>
    /// Verifies that the logger was called at the specified level with a message containing the expected text.
    /// </summary>
    public static void VerifyLoggedContaining<T>(Mock<ILogger<T>> loggerMock, LogLevel level, string expectedText)
    {
        loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedText)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

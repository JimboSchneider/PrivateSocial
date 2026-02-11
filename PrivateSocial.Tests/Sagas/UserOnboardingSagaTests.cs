using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Contracts.Events;
using PrivateSocial.Worker.Sagas;

namespace PrivateSocial.Tests.Sagas;

public class UserOnboardingSagaTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    private ISagaStateMachineTestHarness<UserOnboardingSagaStateMachine, UserOnboardingState> _sagaHarness = null!;

    public async ValueTask InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<UserOnboardingSagaStateMachine, UserOnboardingState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();

        _sagaHarness = _harness.GetSagaStateMachineHarness<UserOnboardingSagaStateMachine, UserOnboardingState>();
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task UserRegistered_ShouldTransitionToOnboardingInProgress()
    {
        // Arrange & Act
        var correlationId = await PublishUserRegisteredAsync(userId: 1, username: "testuser", email: "test@example.com");

        // Assert
        var instance = _sagaHarness.Sagas.ContainsInState(correlationId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.OnboardingInProgress);
        instance.Should().NotBeNull();
        instance!.UserId.Should().Be(1);
        instance.Username.Should().Be("testuser");
        instance.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task UserRegistered_ShouldSendWelcomeEmailAndCreateProfileCommands()
    {
        // Arrange & Act
        var correlationId = await PublishUserRegisteredAsync(userId: 2, username: "newuser", email: "new@example.com");

        // Assert
        (await _harness.Sent.Any<SendWelcomeEmail>(x => x.Context.Message.CorrelationId == correlationId))
            .Should().BeTrue();
        (await _harness.Sent.Any<CreateDefaultProfile>(x => x.Context.Message.CorrelationId == correlationId))
            .Should().BeTrue();
    }

    [Fact]
    public async Task BothStepsComplete_ShouldFinalizeSaga()
    {
        // Arrange
        var correlationId = await PublishUserRegisteredAsync(userId: 3, username: "fullflow", email: "full@example.com");

        // Act — complete both onboarding steps
        await CompleteOnboardingStepsAsync(correlationId, userId: 3, emailFirst: true);

        // Assert
        (await _harness.Published.Any<UserOnboardingCompleted>(
            x => x.Context.Message.CorrelationId == correlationId &&
                 x.Context.Message.UserId == 3)).Should().BeTrue();
    }

    [Fact]
    public async Task ReverseCompletionOrder_ShouldStillFinalize()
    {
        // Arrange
        var correlationId = await PublishUserRegisteredAsync(userId: 4, username: "reverseuser", email: "reverse@example.com");

        // Act — complete in REVERSE order (profile first, then email)
        await CompleteOnboardingStepsAsync(correlationId, userId: 4, emailFirst: false);

        // Assert
        (await _harness.Published.Any<UserOnboardingCompleted>(
            x => x.Context.Message.CorrelationId == correlationId &&
                 x.Context.Message.UserId == 4)).Should().BeTrue();
    }

    [Fact]
    public async Task OnlyOneStepComplete_ShouldNotFinalize()
    {
        // Arrange
        var correlationId = await PublishUserRegisteredAsync(userId: 5, username: "partialuser", email: "partial@example.com");

        // Act — only complete one step
        await _harness.Bus.Publish(new WelcomeEmailSent
        {
            CorrelationId = correlationId,
            UserId = 5,
            SentAt = DateTime.UtcNow
        });
        (await _sagaHarness.Consumed.Any<WelcomeEmailSent>()).Should().BeTrue();

        // Assert — saga should still be in OnboardingInProgress
        var instance = _sagaHarness.Sagas.ContainsInState(correlationId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.OnboardingInProgress);
        instance.Should().NotBeNull();
        instance!.WelcomeEmailSent.Should().BeTrue();
        instance.DefaultProfileCreated.Should().BeFalse();
        instance.CompletedAt.Should().BeNull();

        // UserOnboardingCompleted should NOT have been published
        (await _harness.Published.Any<UserOnboardingCompleted>(
            x => x.Context.Message.CorrelationId == correlationId)).Should().BeFalse();
    }

    private async Task<Guid> PublishUserRegisteredAsync(int userId, string username, string email)
    {
        var correlationId = Guid.NewGuid();

        await _harness.Bus.Publish(new UserRegistered
        {
            CorrelationId = correlationId,
            UserId = userId,
            Username = username,
            Email = email,
            RegisteredAt = DateTime.UtcNow
        });

        (await _sagaHarness.Consumed.Any<UserRegistered>()).Should().BeTrue();
        return correlationId;
    }

    private async Task CompleteOnboardingStepsAsync(Guid correlationId, int userId, bool emailFirst)
    {
        var emailSent = new WelcomeEmailSent { CorrelationId = correlationId, UserId = userId, SentAt = DateTime.UtcNow };
        var profileCreated = new DefaultProfileCreated { CorrelationId = correlationId, UserId = userId, CreatedAt = DateTime.UtcNow };

        if (emailFirst)
        {
            await _harness.Bus.Publish(emailSent);
            await _harness.Bus.Publish(profileCreated);
        }
        else
        {
            await _harness.Bus.Publish(profileCreated);
            await _harness.Bus.Publish(emailSent);
        }

        (await _sagaHarness.Consumed.Any<WelcomeEmailSent>()).Should().BeTrue();
        (await _sagaHarness.Consumed.Any<DefaultProfileCreated>()).Should().BeTrue();
    }
}

using MassTransit;
using PrivateSocial.Contracts.Commands;
using PrivateSocial.Contracts.Events;

namespace PrivateSocial.Worker.Sagas;

public class UserOnboardingSagaStateMachine : MassTransitStateMachine<UserOnboardingState>
{
    public State OnboardingInProgress { get; private set; } = null!;
    public State Completed { get; private set; } = null!;

    public Event<UserRegistered> UserRegisteredEvent { get; private set; } = null!;
    public Event<WelcomeEmailSent> WelcomeEmailSentEvent { get; private set; } = null!;
    public Event<DefaultProfileCreated> DefaultProfileCreatedEvent { get; private set; } = null!;

    public UserOnboardingSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => UserRegisteredEvent, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => WelcomeEmailSentEvent, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => DefaultProfileCreatedEvent, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Initially(
            When(UserRegisteredEvent)
                .Then(context =>
                {
                    context.Saga.UserId = context.Message.UserId;
                    context.Saga.Username = context.Message.Username;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.StartedAt = DateTime.UtcNow;
                })
                .Send(new Uri("queue:send-welcome-email"), context => new SendWelcomeEmail
                {
                    CorrelationId = context.Saga.CorrelationId,
                    UserId = context.Saga.UserId,
                    Username = context.Saga.Username,
                    Email = context.Saga.Email
                })
                .Send(new Uri("queue:create-default-profile"), context => new CreateDefaultProfile
                {
                    CorrelationId = context.Saga.CorrelationId,
                    UserId = context.Saga.UserId,
                    Username = context.Saga.Username,
                    Email = context.Saga.Email
                })
                .TransitionTo(OnboardingInProgress)
        );

        During(OnboardingInProgress,
            When(WelcomeEmailSentEvent)
                .Then(context =>
                {
                    context.Saga.WelcomeEmailSent = true;
                })
                .If(context => context.Saga.WelcomeEmailSent && context.Saga.DefaultProfileCreated,
                    binder => binder
                        .Then(context => context.Saga.CompletedAt = DateTime.UtcNow)
                        .Publish(context => new UserOnboardingCompleted
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            UserId = context.Saga.UserId,
                            Username = context.Saga.Username,
                            CompletedAt = context.Saga.CompletedAt ?? DateTime.UtcNow
                        })
                        .TransitionTo(Completed)
                        .Finalize()),

            When(DefaultProfileCreatedEvent)
                .Then(context =>
                {
                    context.Saga.DefaultProfileCreated = true;
                })
                .If(context => context.Saga.WelcomeEmailSent && context.Saga.DefaultProfileCreated,
                    binder => binder
                        .Then(context => context.Saga.CompletedAt = DateTime.UtcNow)
                        .Publish(context => new UserOnboardingCompleted
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            UserId = context.Saga.UserId,
                            Username = context.Saga.Username,
                            CompletedAt = context.Saga.CompletedAt ?? DateTime.UtcNow
                        })
                        .TransitionTo(Completed)
                        .Finalize())
        );

        SetCompletedWhenFinalized();
    }
}

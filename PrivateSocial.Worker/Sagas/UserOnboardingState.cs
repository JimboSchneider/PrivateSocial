using MassTransit;

namespace PrivateSocial.Worker.Sagas;

public class UserOnboardingState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public int CurrentState { get; set; }

    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public bool WelcomeEmailSent { get; set; }
    public bool DefaultProfileCreated { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

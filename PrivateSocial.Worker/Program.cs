using MassTransit;
using PrivateSocial.Worker.Sagas;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumers(typeof(Program).Assembly);

    x.AddSagaStateMachine<UserOnboardingSagaStateMachine, UserOnboardingState>()
        .InMemoryRepository();

    x.UsingRabbitMq((context, cfg) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("messaging")
            ?? throw new InvalidOperationException(
                "RabbitMQ connection string 'messaging' is not configured. Run via PrivateSocial.AppHost or set ConnectionStrings:messaging in appsettings.");
        cfg.Host(new Uri(connectionString));

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();

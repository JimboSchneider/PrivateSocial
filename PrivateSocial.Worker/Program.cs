using MassTransit;
using PrivateSocial.Worker.Sagas;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("messaging");

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumers(typeof(Program).Assembly);

    x.AddSagaStateMachine<UserOnboardingSagaStateMachine, UserOnboardingState>()
        .InMemoryRepository();

    if (!string.IsNullOrEmpty(connectionString))
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(connectionString));
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    }
});

var host = builder.Build();
host.Run();

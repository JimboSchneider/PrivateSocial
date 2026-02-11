using MassTransit;

namespace PrivateSocial.ApiService.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddApiMessaging(this IServiceCollection services, IHostApplicationBuilder builder)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(typeof(MessagingExtensions).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("messaging")
                    ?? throw new InvalidOperationException(
                        "RabbitMQ connection string 'messaging' is not configured. Run via PrivateSocial.AppHost or set ConnectionStrings:messaging in appsettings.");
                cfg.Host(new Uri(connectionString));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

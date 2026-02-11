using MassTransit;

namespace PrivateSocial.ApiService.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddApiMessaging(this IServiceCollection services, IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("messaging");

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(typeof(MessagingExtensions).Assembly);

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

        return services;
    }
}

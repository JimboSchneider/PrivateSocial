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
                var connectionString = builder.Configuration.GetConnectionString("messaging");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    cfg.Host(new Uri(connectionString));
                }

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.HostedServices;
using WeatherSensors.Client.Options;
using WeatherSensors.Client.Services;

namespace WeatherSensors.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSensors(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SensorConfig>(configuration.GetSection("SensorConfig"));

            services.AddGrpcClient<Service.Protos.SensorEventGenerator.SensorEventGeneratorClient>(
                (provider, options) =>
                {
                    SensorConfig config = provider.GetRequiredService<IOptions<SensorConfig>>().Value;
                    options.Address = new Uri(config.Address);
                });

            services.AddSingleton<ISensorEventQueue, SensorEventQueue>();
            services.AddSingleton<ISensorEventStorage, SensorEventStorage>();
            services.AddSingleton<ISensorEventService, SensorEventService>();

            services.AddHostedService<SensorEventReceiver>();
            services.AddHostedService<SensorEventAggregator>();
            
            return services;
        }
    }
}
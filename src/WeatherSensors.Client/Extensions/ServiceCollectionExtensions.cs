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
        public static IServiceCollection AddSensors(this IServiceCollection services, Action<SensorOptions> configureOptions)
        {
            services
                .Configure(configureOptions)
                .AddGrpcClient<Service.Protos.SensorEventGenerator.SensorEventGeneratorClient>(
                    (provider, options) =>
                    {
                        string address = provider.GetRequiredService<IOptions<SensorOptions>>().Value.Config.Address;
                        options.Address = new Uri(address);
                    });

            services.AddSingleton<ISensorEventQueue, SensorEventQueue>();
            services.AddSingleton<ISensorEventStorage, SensorEventStorage>();
            services.AddSingleton<ISensorEventService, SensorEventService>();

            services.Configure(configureOptions).AddHostedService<SensorEventReceiver>();
            services.Configure(configureOptions).AddHostedService<SensorEventAggregator>();

            return services;
        }
    }
}
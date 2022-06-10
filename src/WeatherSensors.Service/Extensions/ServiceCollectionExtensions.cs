using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.HostedServices;
using WeatherSensors.Service.Options;
using WeatherSensors.Service.Services;

namespace WeatherSensors.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSensors(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SensorConfig>(configuration.GetSection("SensorConfig"));

            Type[] sensorTypes = Assembly.GetAssembly(typeof(ISensor))
                ?.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(ISensor)) && t.IsClass && !t.IsAbstract)
                .ToArray() ?? Array.Empty<Type>();
            foreach (Type sensorType in sensorTypes)
            {
                services.AddSingleton(typeof(ISensor), sensorType);
            }

            services.AddSingleton<ISensorEventCache, SensorEventCache>();
            services.AddSingleton<ISensorEventBus, SensorEventBus>();

            services.AddHostedService<SensorEventGeneratorService>();

            return services;
        }
    }
}
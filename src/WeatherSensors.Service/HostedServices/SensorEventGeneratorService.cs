using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Extensions;
using WeatherSensors.Service.Models;
using WeatherSensors.Service.Options;

namespace WeatherSensors.Service.HostedServices
{
    public sealed class SensorEventGeneratorService : BackgroundService
    {
        private readonly SensorConfig _sensorConfig;
        private readonly ILogger<SensorEventGeneratorService> _logger;
        private readonly IEnumerable<ISensor> _sensors;
        private readonly ISensorEventCache _sensorEventCache;
        private readonly ISensorEventBus _sensorEventBus;

        public SensorEventGeneratorService(
            ILogger<SensorEventGeneratorService> logger,
            IOptions<SensorConfig> options,
            IEnumerable<ISensor> sensors,
            ISensorEventCache sensorEventCache,
            ISensorEventBus sensorEventBus)
        {
            _sensorConfig = options.Value;
            _logger = logger;
            _sensors = sensors;
            _sensorEventCache = sensorEventCache;
            _sensorEventBus = sensorEventBus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(_sensors.Select(s => ProcessSensorDataAsync(s, stoppingToken)));

                await Task.Delay(_sensorConfig.IntervalMilliseconds, stoppingToken);
            }
        }

        private async Task ProcessSensorDataAsync(ISensor sensor, CancellationToken ct)
        {
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(_sensorConfig.TimeoutMilliseconds);
                SensorData sensorData = await sensor.GetSensorDataAsync(ct).WaitAsync(timeout, ct);
                SensorEvent sensorEvent = new SensorEvent
                {
                    SensorKey = sensor.SensorKey,
                    SensorData = sensorData,
                    CreatedAt = DateTimeOffset.Now
                };
                _sensorEventCache.AddEvent(sensorEvent);
                _sensorEventBus.Publish(sensorEvent);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError(e, "ProcessSensorDataAsync failed for sensor '{0}'", sensor.SensorKey);
            }
        }
    }
}
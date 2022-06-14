using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Models;
using WeatherSensors.Service.Options;

namespace WeatherSensors.Service.HostedServices
{
    public sealed class SensorEventGenerator : BackgroundService
    {
        private readonly ILogger<SensorEventGenerator> _logger;
        private readonly IEnumerable<ISensor> _sensors;
        private readonly ISensorEventCache _sensorEventCache;
        private readonly ISensorEventBus _sensorEventBus;
        private readonly TimeSpan _interval;
        private readonly TimeSpan _timeout;

        public SensorEventGenerator(
            ILogger<SensorEventGenerator> logger,
            IOptions<SensorOptions> options,
            IEnumerable<ISensor> sensors,
            ISensorEventCache sensorEventCache,
            ISensorEventBus sensorEventBus)
        {
            _logger = logger;
            _sensors = sensors;
            _sensorEventCache = sensorEventCache;
            _sensorEventBus = sensorEventBus;
            _interval = TimeSpan.FromMilliseconds(options.Value.Config.IntervalMilliseconds);
            _timeout = TimeSpan.FromMilliseconds(options.Value.Config.TimeoutMilliseconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.WhenAll(_sensors.Select(s => ProcessSensorDataAsync(s, stoppingToken)));

                    await Task.Delay(_interval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Service stopped working due operation was canceled");
            }
        }

        private async Task ProcessSensorDataAsync(ISensor sensor, CancellationToken ct)
        {
            try
            {
                SensorData sensorData = await sensor.GetSensorDataAsync(ct).WaitAsync(_timeout, ct);
                SensorEvent sensorEvent = new SensorEvent
                {
                    SensorKey = sensor.SensorKey,
                    SensorData = sensorData,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _sensorEventCache.AddEvent(sensorEvent);
                _sensorEventBus.Publish(sensorEvent);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get sensor '{0}' data failed", sensor.SensorKey);
            }
        }
    }
}
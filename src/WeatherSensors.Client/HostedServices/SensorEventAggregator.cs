using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.Models;
using WeatherSensors.Client.Options;

namespace WeatherSensors.Client.HostedServices
{
    public class SensorEventAggregator : BackgroundService
    {
        private readonly ILogger<SensorEventAggregator> _logger;
        private readonly ISensorEventQueue _sensorEventQueue;
        private readonly ISensorEventStorage _sensorEventStorage;
        private readonly SensorConfig _sensorConfig;

        public SensorEventAggregator(
            ILogger<SensorEventAggregator> logger,
            IOptions<SensorConfig> options,
            ISensorEventQueue sensorEventQueue,
            ISensorEventStorage sensorEventStorage)
        {
            _logger = logger;
            _sensorEventQueue = sensorEventQueue;
            _sensorEventStorage = sensorEventStorage;
            _sensorConfig = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                TimeSpan waitBeforeStart = TimeSpan.FromSeconds((60 - DateTimeOffset.Now.TimeOfDay.Seconds) % 60);
                TimeSpan waitBetweenIterations = TimeSpan.FromMinutes(_sensorConfig.AggregationIntervalMinutes);

                await Task.Delay(waitBeforeStart, stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    IEnumerable<IGrouping<string, SensorEvent>> sensorEventGroups = _sensorEventQueue
                        .DequeEvents(DateTimeOffset.Now)
                        .GroupBy(e => e.SensorKey);
                    foreach (IGrouping<string, SensorEvent> group in sensorEventGroups)
                    {
                        SensorEvent[] sensorEvents = group.OrderBy(e => e.CreatedAt).ToArray();
                        if (sensorEvents.Any())
                        {
                            AggregatedSensorEvent aggregatedSensorEvent = new()
                            {
                                SensorKey = group.Key,
                                AverageTemperature = sensorEvents.Average(t => t.Temperature),
                                AverageHumidity = (int)sensorEvents.Average(t => t.Humidity),
                                MinCarbonDioxideLevel = sensorEvents.Min(t => t.CarbonDioxideLevel),
                                MaxCarbonDioxideLevel = sensorEvents.Max(t => t.CarbonDioxideLevel),
                                StartedAt = sensorEvents.First().CreatedAt,
                                EndedAt = sensorEvents.Last().CreatedAt
                            };
                            _sensorEventStorage.AddEvent(aggregatedSensorEvent);

                            _logger.LogInformation(aggregatedSensorEvent.ToString());
                        }
                    }
                    
                    await Task.Delay(waitBetweenIterations, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SensorEventAggregator stopped working due operation was canceled");
            }
            catch (Exception e)
            {
                _logger.LogError("SensorEventAggregator stopped working", e);
            }
        }
    }
}
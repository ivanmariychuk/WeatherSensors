using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.Models;
using WeatherSensors.Client.Options;

namespace WeatherSensors.Client.HostedServices
{
    public class SensorEventReceiver : BackgroundService
    {
        private readonly ILogger<SensorEventReceiver> _logger;
        private readonly ISensorEventQueue _sensorEventQueue;
        private readonly ISensorEventService _sensorEventService;
        private readonly SensorConfig _sensorConfig;

        public SensorEventReceiver(
            ILogger<SensorEventReceiver> logger,
            IOptions<SensorConfig> options,
            ISensorEventQueue sensorEventQueue,
            ISensorEventService sensorEventService)
        {
            _logger = logger;
            _sensorEventQueue = sensorEventQueue;
            _sensorEventService = sensorEventService;
            _sensorConfig = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await foreach (SensorEvent sensorEvent in _sensorEventService.ReadAllAsync(stoppingToken))
                        {
                            stoppingToken.ThrowIfCancellationRequested();

                            _sensorEventQueue.EnqueueEvent(sensorEvent);
                        }
                    }
                    catch (RpcException e)
                    {
                        _logger.LogError(e, null);

                        if (!await _sensorEventService.TryRestartAsync())
                        {
                            await Task.Delay(_sensorConfig.RetryIntervalSeconds, stoppingToken);
                        }

                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SensorEventReceiver stopped working due operation was canceled");
            }
            catch (Exception e)
            {
                _logger.LogError("SensorEventReceiver stopped working", e);
            }
        }
    }
}
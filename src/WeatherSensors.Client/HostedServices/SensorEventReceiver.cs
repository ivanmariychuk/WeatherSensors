using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.Models;
using WeatherSensors.Client.Options;

namespace WeatherSensors.Client.HostedServices
{
    public sealed class SensorEventReceiver : BackgroundService
    {
        private readonly ILogger<SensorEventReceiver> _logger;
        private readonly ISensorEventQueue _sensorEventQueue;
        private readonly ISensorEventService _sensorEventService;
        private readonly SensorConfig _sensorConfig;

        public SensorEventReceiver(
            ILogger<SensorEventReceiver> logger,
            IOptions<SensorOptions> options,
            ISensorEventQueue sensorEventQueue,
            ISensorEventService sensorEventService)
        {
            _logger = logger;
            _sensorEventQueue = sensorEventQueue;
            _sensorEventService = sensorEventService;
            _sensorConfig = options.Value.Config;
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
                        
                        await ReconnectAsync(stoppingToken);
                    }
                    catch (RpcException e) when (e.StatusCode == StatusCode.Unavailable)
                    {
                        _logger.LogWarning("GPRC Service is unavailable, trying to reconnect");

                        await ReconnectAsync(stoppingToken);
                    }
                    catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
                    {
                        _logger.LogWarning("GPRC Service operation was cancelled by client");
                        
                        await _sensorEventService.CompleteAsync();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Service stopped working due operation was canceled");

                await _sensorEventService.CompleteAsync();
            }
        }

        private async Task ReconnectAsync(CancellationToken ct)
        {
            while (!await _sensorEventService.TryReconnectAsync())
            {
                await Task.Delay(_sensorConfig.RetryIntervalSeconds, ct);
            }
        }
    }
}
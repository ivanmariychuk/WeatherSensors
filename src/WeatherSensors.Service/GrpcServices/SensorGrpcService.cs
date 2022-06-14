using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherSensors.Service.Protos;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Extensions;

namespace WeatherSensors.Service.GrpcServices
{
    public sealed class SensorGrpcService : SensorEventGenerator.SensorEventGeneratorBase
    {
        private readonly ILogger<SensorGrpcService> _logger;
        private readonly ISensorEventBus _sensorEventBus;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly string[] _allSensors;
        private readonly ConcurrentDictionary<string, bool> _requestedSensors;

        public SensorGrpcService(
            ILogger<SensorGrpcService> logger,
            ISensorEventBus sensorEventBus,
            IEnumerable<ISensor> sensors,
            IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _sensorEventBus = sensorEventBus;
            _lifetime = lifetime;
            _allSensors = sensors.Select(s => s.SensorKey).ToArray();
            _requestedSensors = new();
        }

        public override Task<GetSensorsResponse> GetSensors(Empty request, ServerCallContext context)
        {
            GetSensorsResponse response = new();
            response.Sensors.AddRange(_allSensors);
            return Task.FromResult(response);
        }

        public override async Task SensorEventStream(
            IAsyncStreamReader<SensorEventRequest> requestStream,
            IServerStreamWriter<SensorEventResponse> responseStream,
            ServerCallContext context)
        {
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                _lifetime.ApplicationStopping,
                context.CancellationToken);

            IDisposable sensorEventSub = null;
            try
            {
                sensorEventSub = _sensorEventBus
                    .AsObservable()
                    .Where(e => _requestedSensors.GetOrAdd(e.SensorKey, false))
                    .SelectMany(e =>
                        responseStream
                            .WriteAsync(e.ToSensorEventResponse(), cts.Token)
                            .ToObservable())
                    .Subscribe();

                await foreach (SensorEventRequest request in requestStream.ReadAllAsync(cts.Token))
                {
                    _logger.LogInformation("Command {0} received", request.Command);

                    switch (request.Command)
                    {
                        case SensorEventCommand.Subscribe:
                            Subscribe(request.Sensors);
                            break;
                        case SensorEventCommand.SubscribeAll:
                            Subscribe(_allSensors);
                            break;
                        case SensorEventCommand.Unsubscribe:
                            Unsubscribe(request.Sensors);
                            break;
                        case SensorEventCommand.UnsubscribeAll:
                            Unsubscribe(_allSensors);
                            break;
                        default:
                            continue;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Service stopped working due operation was canceled");
            }
            finally
            {
                sensorEventSub?.Dispose();
            }
        }

        private void Subscribe(IEnumerable<string> sensors)
        {
            foreach (string sensor in sensors)
            {
                _requestedSensors.AddOrUpdate(sensor, true, (_, _) => true);
            }
        }

        private void Unsubscribe(IEnumerable<string> sensors)
        {
            foreach (string sensor in sensors)
            {
                _requestedSensors.AddOrUpdate(sensor, false, (_, _) => false);
            }
        }
    }
}
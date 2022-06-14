using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.Extensions;
using WeatherSensors.Client.Models;
using WeatherSensors.Service.Protos;

namespace WeatherSensors.Client.Services
{
    public sealed class SensorEventService : ISensorEventService, IDisposable
    {
        private readonly SensorEventGenerator.SensorEventGeneratorClient _sensorEventGeneratorClient;
        private readonly TaskCompletionSource _sensorEventStreamConnected;
        private readonly HashSet<string> _requestedSensors;
        private AsyncDuplexStreamingCall<SensorEventRequest, SensorEventResponse> _eventStream;

        public SensorEventService(SensorEventGenerator.SensorEventGeneratorClient sensorEventGeneratorClient)
        {
            _sensorEventGeneratorClient = sensorEventGeneratorClient;
            _sensorEventStreamConnected = new TaskCompletionSource();
            _requestedSensors = new HashSet<string>();
        }

        public async IAsyncEnumerable<SensorEvent> ReadAllAsync([EnumeratorCancellation] CancellationToken ct)
        {
            await _sensorEventStreamConnected.Task;

            await foreach (SensorEventResponse response in _eventStream.ResponseStream.ReadAllAsync(ct))
            {
                yield return response.ToSensorEvent();
            }
        }

        public async Task SubscribeAsync(IReadOnlyCollection<string> sensors)
        {
            EnsureConnected();

            SensorEventRequest request = new() { Command = SensorEventCommand.Subscribe };
            request.Sensors.AddRange(sensors);

            await _eventStream.RequestStream.WriteAsync(request);

            _requestedSensors.UnionWith(sensors);
        }

        public async Task SubscribeAllAsync()
        {
            EnsureConnected();

            GetSensorsResponse response = await _sensorEventGeneratorClient.GetSensorsAsync(new Empty()).ResponseAsync;
            SensorEventRequest request = new() { Command = SensorEventCommand.SubscribeAll };
            request.Sensors.AddRange(response.Sensors);

            await _eventStream.RequestStream.WriteAsync(request);

            _requestedSensors.UnionWith(response.Sensors);
        }

        public async Task UnsubscribeAsync(IReadOnlyCollection<string> sensors)
        {
            EnsureConnected();

            SensorEventRequest request = new() { Command = SensorEventCommand.Unsubscribe };
            request.Sensors.AddRange(sensors);

            await _eventStream.RequestStream.WriteAsync(request);

            _requestedSensors.ExceptWith(sensors);
        }

        public async Task UnsubscribeAllAsync()
        {
            EnsureConnected();

            SensorEventRequest request = new() { Command = SensorEventCommand.UnsubscribeAll };

            await _eventStream.RequestStream.WriteAsync(request);

            _requestedSensors.Clear();
        }

        public async Task<bool> TryReconnectAsync()
        {
            try
            {
                _eventStream = _sensorEventGeneratorClient.SensorEventStream();

                await SubscribeAsync(_requestedSensors);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task CompleteAsync()
        {
            await _eventStream.RequestStream.CompleteAsync();
            Dispose();
        }

        public void Dispose()
        {
            _eventStream?.Dispose();
        }

        private void EnsureConnected()
        {
            _eventStream ??= _sensorEventGeneratorClient.SensorEventStream();
            if (!_sensorEventStreamConnected.Task.IsCompleted)
            {
                _sensorEventStreamConnected.SetResult();
            }
        }
    }
}
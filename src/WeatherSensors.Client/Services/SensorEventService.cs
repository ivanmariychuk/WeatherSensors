using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.Extensions;
using WeatherSensors.Client.Models;
using WeatherSensors.Service.Protos;

namespace WeatherSensors.Client.Services
{
    public class SensorEventService : ISensorEventService
    {
        private readonly SensorEventGenerator.SensorEventGeneratorClient _sensorEventGeneratorClient;
        private readonly HashSet<string> _requestedSensors;
        private AsyncDuplexStreamingCall<SensorEventRequest, SensorEventResponse> _eventStream;

        public SensorEventService(SensorEventGenerator.SensorEventGeneratorClient sensorEventGeneratorClient)
        {
            _sensorEventGeneratorClient = sensorEventGeneratorClient;
            _requestedSensors = new();
            _eventStream = sensorEventGeneratorClient.SensorEventStream();
        }

        public async IAsyncEnumerable<SensorEvent> ReadAllAsync([EnumeratorCancellation] CancellationToken ct)
        {
            await foreach (SensorEventResponse response in _eventStream.ResponseStream.ReadAllAsync(ct))
            {
                yield return response.ToSensorEvent();
            }
        }

        public async Task SubscribeAsync(IEnumerable<string> sensors)
        {
            SensorEventRequest request = new() { Command = SensorEventCommand.Subscribe };
            foreach (string sensor in sensors)
            {
                request.Sensors.Add(sensor);

                _requestedSensors.Add(sensor);
            }

            await _eventStream.RequestStream.WriteAsync(request);
        }

        public async Task SubscribeAllAsync()
        {
            SensorEventRequest request = new() { Command = SensorEventCommand.SubscribeAll };
            foreach (string sensor in _sensorEventGeneratorClient.GetSensors(new Empty()).Sensors)
            {
                _requestedSensors.Add(sensor);
            }

            await _eventStream.RequestStream.WriteAsync(request);
        }

        public async Task UnsubscribeAsync(IEnumerable<string> sensors)
        {
            SensorEventRequest request = new() { Command = SensorEventCommand.Unsubscribe };
            foreach (string sensor in sensors)
            {
                request.Sensors.Add(sensor);

                _requestedSensors.Remove(sensor);
            }

            await _eventStream.RequestStream.WriteAsync(request);
        }

        public async Task UnsubscribeAllAsync()
        {
            SensorEventRequest request = new() { Command = SensorEventCommand.UnsubscribeAll };
            _requestedSensors.Clear();

            await _eventStream.RequestStream.WriteAsync(request);
        }

        public Task CompleteAsync()
        {
            return _eventStream.RequestStream.CompleteAsync();
        }

        public async Task<bool> TryRestartAsync()
        {
            try
            {
                _eventStream?.Dispose();
                _eventStream = _sensorEventGeneratorClient.SensorEventStream();
                
                await SubscribeAsync(_requestedSensors);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
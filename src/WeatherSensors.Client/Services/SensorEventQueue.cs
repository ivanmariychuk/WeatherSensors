using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.Models;

namespace WeatherSensors.Client.Services
{
    public sealed class SensorEventQueue : ISensorEventQueue
    {
        private readonly ConcurrentQueue<SensorEvent> _sensorEvents = new();

        public void EnqueueEvent(SensorEvent sensorEvent)
        {
            _sensorEvents.Enqueue(sensorEvent);
        }

        public IEnumerable<SensorEvent> DequeEvents(DateTimeOffset until)
        {
            while (_sensorEvents.TryPeek(out SensorEvent sensorEvent) && sensorEvent.CreatedAt < until)
            {
                if (_sensorEvents.TryDequeue(out sensorEvent))
                {
                    yield return sensorEvent;
                }
            }
        }
    }
}
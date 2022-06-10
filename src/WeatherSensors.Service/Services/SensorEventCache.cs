using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Services
{
    public sealed class SensorEventCache : ISensorEventCache
    {
        private readonly ConcurrentDictionary<string, SensorEvent> _sensorEvents = new();

        public IEnumerable<SensorEvent> GetEvents()
        {
            return _sensorEvents.Values.Where(e => e is not null).OrderByDescending(e => e.CreatedAt);
        }

        public SensorEvent GetEvent(string sensorKey)
        {
            return _sensorEvents.TryGetValue(sensorKey, out SensorEvent sensorEvent) ? sensorEvent : null;
        }

        public void AddEvent(SensorEvent sensorEvent)
        {
            _sensorEvents.AddOrUpdate(sensorEvent.SensorKey, sensorEvent, (_, _) => sensorEvent);
        }
    }
}
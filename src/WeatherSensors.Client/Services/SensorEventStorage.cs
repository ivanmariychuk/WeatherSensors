using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.Models;

namespace WeatherSensors.Client.Services
{
    public sealed class SensorEventStorage : ISensorEventStorage
    {
        private readonly ConcurrentDictionary<string, List<AggregatedSensorEvent>> _sensorEvents = new();

        public IEnumerable<AggregatedSensorEvent> GetAllEvents()
        {
            return _sensorEvents.Values
                .SelectMany(e => e)
                .OrderByDescending(e => e.StartedAt);
        }

        public IEnumerable<AggregatedSensorEvent> GetEvents(DateTimeOffset start, DateTimeOffset end)
        {
            return _sensorEvents.Keys.Select(k => GetEvent(k, start, end)).Where(e => e is not null);
        }

        public AggregatedSensorEvent GetEvent(string sensorKey, DateTimeOffset start, DateTimeOffset end)
        {
            AggregatedSensorEvent[] sensorEvents = _sensorEvents[sensorKey]
                .Where(e => e.StartedAt >= start && e.EndedAt < end)
                .OrderBy(e => e.StartedAt)
                .ThenBy(e => e.EndedAt)
                .ToArray();
            if (sensorEvents.Length == 0)
            {
                return null;
            }

            double averageTemperature = sensorEvents.Average(e => e.AverageTemperature);
            int averageHumidity = (int)sensorEvents.Average(e => e.AverageHumidity);
            int minCo2Level = sensorEvents.Min(e => e.MinCarbonDioxideLevel);
            int maxCo2Level = sensorEvents.Max(e => e.MaxCarbonDioxideLevel);
            AggregatedSensorEvent sensorEvent = new()
            {
                SensorKey = sensorKey,
                AverageTemperature = averageTemperature,
                AverageHumidity = averageHumidity,
                MinCarbonDioxideLevel = minCo2Level,
                MaxCarbonDioxideLevel = maxCo2Level,
                StartedAt = sensorEvents.First().StartedAt,
                EndedAt = sensorEvents.Last().EndedAt
            };
            return sensorEvent;
        }

        public void AddEvent(AggregatedSensorEvent sensorEvent)
        {
            _sensorEvents.AddOrUpdate(
                sensorEvent.SensorKey,
                new List<AggregatedSensorEvent> { sensorEvent },
                (_, list) =>
                {
                    lock (list)
                    {
                        list.Add(sensorEvent);
                        return list;
                    }
                });
        }
        
    }
}
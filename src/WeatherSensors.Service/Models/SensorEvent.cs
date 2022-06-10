using System;

namespace WeatherSensors.Service.Models
{
    public sealed record SensorEvent
    {
        public string SensorKey { get; init; }
        public SensorData SensorData { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
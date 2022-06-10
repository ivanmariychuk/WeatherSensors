using System;

namespace WeatherSensors.Client.Models
{
    public sealed record SensorEvent
    {
        public string SensorKey { get; init; }
        public double Temperature { get; init; }
        public int Humidity { get; init; }
        public int CarbonDioxideLevel { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
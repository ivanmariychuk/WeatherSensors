using System;

namespace WeatherSensors.Client.Models
{
    public sealed record AggregatedSensorEvent
    {
        public string SensorKey { get; init; }
        public double AverageTemperature { get; init; }
        public int AverageHumidity { get; init; }
        public int MinCarbonDioxideLevel { get; init; }
        public int MaxCarbonDioxideLevel { get; init; }
        public DateTimeOffset StartedAt { get; init; }
        public DateTimeOffset EndedAt { get; init; }
    }
}
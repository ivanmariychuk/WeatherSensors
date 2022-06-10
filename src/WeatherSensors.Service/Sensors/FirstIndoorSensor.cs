using System;
using System.Threading;
using System.Threading.Tasks;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Sensors
{
    public sealed class FirstIndoorSensor : ISensor
    {
        private readonly Random _random = new();

        public string SensorKey => "first_indoor_sensor";

        public Task<SensorData> GetSensorDataAsync(CancellationToken ct)
        {
            return Task.FromResult(new SensorData
            {
                Temperature = 20 * (0.5 + _random.NextDouble() / 4),
                Humidity = _random.Next(25, 75),
                CarbonDioxideLevel = _random.Next(500, 2000)
            });
        }
    }
}
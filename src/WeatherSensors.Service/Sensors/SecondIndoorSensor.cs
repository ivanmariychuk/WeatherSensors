using System;
using System.Threading;
using System.Threading.Tasks;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Sensors
{
    public sealed class SecondIndoorSensor : ISensor
    {
        private readonly Random _random = new();

        public string SensorKey => "second_indoor_sensor";

        public Task<SensorData> GetSensorDataAsync(CancellationToken ct)
        {
            return Task.FromResult(new SensorData
            {
                Temperature = 20 * (0.5 + _random.NextDouble() / 4),
                Humidity = _random.Next(30, 60),
                CarbonDioxideLevel = _random.Next(500, 1200)
            });
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Sensors
{
    public sealed class OutdoorSensor : ISensor
    {
        private readonly Random _random = new();

        public string SensorKey => "outdoor_sensor";

        public Task<SensorData> GetSensorDataAsync(CancellationToken ct)
        {
            return Task.FromResult(new SensorData
            {
                Temperature = 25 * _random.NextDouble(),
                Humidity = _random.Next(35, 100),
                CarbonDioxideLevel = _random.Next(400, 500)
            });
        }
    }
}
using WeatherSensors.Client.Models;
using WeatherSensors.Service.Protos;

namespace WeatherSensors.Client.Extensions
{
    public static class SensorEventExtensions
    {
        public static SensorEvent ToSensorEvent(this SensorEventResponse response)
        {
            return new()
            {
                SensorKey = response.Sensor,
                Temperature = response.Temperature,
                Humidity = response.Humidity,
                CarbonDioxideLevel = response.CarbonDioxideLevel,
                CreatedAt = response.CreatedAt.ToDateTimeOffset()
            };
        }
    }
}
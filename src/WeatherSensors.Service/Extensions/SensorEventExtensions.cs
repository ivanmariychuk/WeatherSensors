using Google.Protobuf.WellKnownTypes;
using WeatherSensors.Service.Protos;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Extensions
{
    public static class SensorEventExtensions
    {
        public static SensorEventResponse ToSensorEventResponse(this SensorEvent sensorEvent) =>
            new()
            {
                Sensor = sensorEvent.SensorKey,
                Temperature = sensorEvent.SensorData.Temperature,
                Humidity = sensorEvent.SensorData.Humidity,
                CarbonDioxideLevel = sensorEvent.SensorData.CarbonDioxideLevel,
                CreatedAt = sensorEvent.CreatedAt.ToTimestamp()
            };
    }
}
using System.Text.Json.Serialization;

namespace WeatherSensors.Client.Models
{
    public sealed class SensorUnsubscribeRequest
    {
        [JsonPropertyName("all_sensors")]
        public bool AllSensors { get; init; }

        [JsonPropertyName("sensors")]
        public string[] Sensors { get; init; }
    }
}
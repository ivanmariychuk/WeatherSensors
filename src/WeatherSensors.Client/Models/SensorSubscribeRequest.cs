namespace WeatherSensors.Client.Models
{
    public class SensorSubscribeRequest
    {
        public bool All { get; set; }
        public string[] Sensors { get; set; }
    }
}
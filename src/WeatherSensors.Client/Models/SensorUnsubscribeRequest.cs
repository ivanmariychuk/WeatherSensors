namespace WeatherSensors.Client.Models
{
    public class SensorUnsubscribeRequest
    {
        public bool All { get; set; }
        public string[] Sensors { get; set; }
    }
}
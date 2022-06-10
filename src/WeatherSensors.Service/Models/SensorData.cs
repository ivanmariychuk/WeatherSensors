namespace WeatherSensors.Service.Models
{
    public readonly struct SensorData
    {
        public double Temperature { get; init; }
        public int Humidity { get; init; }
        public int CarbonDioxideLevel { get; init; }
    }
}
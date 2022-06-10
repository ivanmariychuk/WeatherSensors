using System.Threading;
using System.Threading.Tasks;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Abstractions
{
    public interface ISensor
    {
        string SensorKey { get; }

        Task<SensorData> GetSensorDataAsync(CancellationToken ct);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WeatherSensors.Client.Models;

namespace WeatherSensors.Client.Abstractions
{
    public interface ISensorEventService
    {
        IAsyncEnumerable<SensorEvent> ReadAllAsync(CancellationToken ct);
        Task SubscribeAsync(IEnumerable<string> sensors);
        Task SubscribeAllAsync();
        Task UnsubscribeAsync(IEnumerable<string> sensors);
        Task UnsubscribeAllAsync();
        Task CompleteAsync();
        Task<bool> TryRestartAsync();
    }
}
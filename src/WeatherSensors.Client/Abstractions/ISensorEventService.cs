using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WeatherSensors.Client.Models;

namespace WeatherSensors.Client.Abstractions
{
    public interface ISensorEventService
    {
        IAsyncEnumerable<SensorEvent> ReadAllAsync(CancellationToken ct);
        Task SubscribeAsync(IReadOnlyCollection<string> sensors);
        Task SubscribeAllAsync();
        Task UnsubscribeAsync(IReadOnlyCollection<string> sensors);
        Task UnsubscribeAllAsync();
        Task CompleteAsync();
        Task<bool> TryReconnectAsync();
    }
}
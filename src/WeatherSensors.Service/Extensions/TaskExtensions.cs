using System;
using System.Threading;
using System.Threading.Tasks;

namespace WeatherSensors.Service.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<T>();
            await using (new Timer(s => ((TaskCompletionSource<T>)s)?.TrySetException(new TimeoutException()),
                tcs, timeout, Timeout.InfiniteTimeSpan))
            await using (ct.Register(s => ((TaskCompletionSource<T>)s).TrySetCanceled(), tcs))
            {
                return await (await Task.WhenAny(task, tcs.Task).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }
    }
}
using System;

namespace WeatherSensors.Client.Options
{
    public sealed class SensorConfig
    {
        private readonly int _aggregationIntervalMinutes;
        private readonly int _retryIntervalSeconds;

        public string Address { get; set; }

        public int AggregationIntervalMinutes
        {
            get => _aggregationIntervalMinutes;
            init
            {
                if (value is < 1 or > 60)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                _aggregationIntervalMinutes = value;
            }
        }

        public int RetryIntervalSeconds
        {
            get => _retryIntervalSeconds;
            init
            {
                if (value is < 1 or > 60)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                _retryIntervalSeconds = value;
            }
        }
    }
}
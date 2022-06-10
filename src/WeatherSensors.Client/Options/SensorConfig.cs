using System;

namespace WeatherSensors.Client.Options
{
    public class SensorConfig
    {
        private int _aggregationIntervalMinutes;
        private int _retryIntervalSeconds;

        public string Address { get; set; }

        public int AggregationIntervalMinutes
        {
            get => _aggregationIntervalMinutes;
            set
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
            set
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
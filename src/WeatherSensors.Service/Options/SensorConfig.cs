using System;

namespace WeatherSensors.Service.Options
{
    public sealed class SensorConfig
    {
        private readonly int _timeoutMilliseconds;
        private readonly int _intervalMilliseconds;

        public int IntervalMilliseconds
        {
            get => _intervalMilliseconds;
            init
            {
                if (value is < 100 or > 2000)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                _intervalMilliseconds = value;
            }
        }

        public int TimeoutMilliseconds
        {
            get => _timeoutMilliseconds;
            init
            {
                if (value is < 10 or > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                _timeoutMilliseconds = value;
            }
        }
    }
}
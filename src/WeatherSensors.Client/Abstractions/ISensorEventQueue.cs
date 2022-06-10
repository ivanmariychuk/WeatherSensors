using System;
using System.Collections.Generic;
using WeatherSensors.Client.Models;

namespace WeatherSensors.Client.Abstractions
{
    public interface ISensorEventQueue
    {
        void EnqueueEvent(SensorEvent sensorEvent);
        IEnumerable<SensorEvent> DequeEvents(DateTimeOffset until);
    }
}
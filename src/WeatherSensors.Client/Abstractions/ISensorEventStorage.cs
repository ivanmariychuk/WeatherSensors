using System;
using System.Collections.Generic;
using WeatherSensors.Client.Models;

namespace WeatherSensors.Client.Abstractions
{
    public interface ISensorEventStorage
    {
        IEnumerable<AggregatedSensorEvent> GetAllEvents();
        IEnumerable<AggregatedSensorEvent> GetEvents(DateTimeOffset start, DateTimeOffset end);
        AggregatedSensorEvent GetEvent(string sensorKey, DateTimeOffset start, DateTimeOffset end);
        void AddEvent(AggregatedSensorEvent sensorEvent);
    }
}
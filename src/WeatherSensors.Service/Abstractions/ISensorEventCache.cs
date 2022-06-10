using System.Collections.Generic;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Abstractions
{
    public interface ISensorEventCache
    {
        IEnumerable<SensorEvent> GetEvents();
        SensorEvent GetEvent(string sensorKey);
        void AddEvent(SensorEvent sensorEvent);
    }
}
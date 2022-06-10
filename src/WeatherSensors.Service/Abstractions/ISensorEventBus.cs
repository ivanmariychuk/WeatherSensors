using System;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Abstractions
{
    public interface ISensorEventBus
    {
        void Publish(SensorEvent sensorEvent);
        IObservable<SensorEvent> AsObservable();
    }
}
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Services
{
    public class SensorEventBus : ISensorEventBus
    {
        private readonly Subject<SensorEvent> _sensorEventSubject = new();

        public void Publish(SensorEvent sensorEvent)
        {
            _sensorEventSubject.OnNext(sensorEvent);
        }

        public IObservable<SensorEvent> AsObservable()
        {
            return _sensorEventSubject.AsObservable();
        }
    }
}
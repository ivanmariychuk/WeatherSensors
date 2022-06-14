using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Services
{
    public sealed class SensorEventBus : ISensorEventBus
    {
        private readonly ISubject<SensorEvent> _sensorEventSubject = Subject.Synchronize(new Subject<SensorEvent>());

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
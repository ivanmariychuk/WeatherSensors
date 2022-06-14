using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WeatherSensors.Service.Abstractions;
using WeatherSensors.Service.Models;

namespace WeatherSensors.Service.Controllers
{
    [ApiController, Route("api/sensors")]
    public sealed class SensorController : ControllerBase
    {
        private readonly ISensorEventCache _sensorEventCache;

        public SensorController(ISensorEventCache sensorEventCache)
        {
            _sensorEventCache = sensorEventCache;
        }

        // GET api/sensors
        [HttpGet]
        public ActionResult<SensorEvent[]> GetAllSensorsData()
        {
            IEnumerable<SensorEvent> sensorEvents = _sensorEventCache.GetEvents().OrderBy(e => e.SensorKey);
            return Ok(sensorEvents);
        }

        // GET api/sensors/sensor_key
        [HttpGet("{sensorKey}")]
        public ActionResult<SensorEvent> GetSensorData(string sensorKey)
        {
            SensorEvent sensorEvent = _sensorEventCache.GetEvent(sensorKey);
            if (sensorEvent is null)
            {
                return NotFound(sensorKey);
            }

            return Ok(sensorEvent);
        }
    }
}
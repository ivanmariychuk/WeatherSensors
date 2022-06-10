using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WeatherSensors.Client.Abstractions;
using WeatherSensors.Client.Models;

namespace WeatherSensors.Client.Controllers
{
    [ApiController, Route("api/sensors")]
    public class SensorController : ControllerBase
    {
        private readonly ISensorEventService _sensorEventService;
        private readonly ISensorEventStorage _sensorEventStorage;

        public SensorController(
            ISensorEventService sensorEventService,
            ISensorEventStorage sensorEventStorage)
        {
            _sensorEventService = sensorEventService;
            _sensorEventStorage = sensorEventStorage;
        }

        // GET api/sensors/aggregate?start=2022-06-10T00:00&end=2022-06-10T00:10
        [HttpGet("aggregate")]
        public ActionResult GetSensorEvents(DateTimeOffset start, DateTimeOffset end)
        {
            IEnumerable<AggregatedSensorEvent> sensorEvents = _sensorEventStorage.GetEvents(start, end);

            return Ok(sensorEvents);
        }
        
        // GET api/sensors/aggregate/sensor_key?start=2022-06-10T00:00&end=2022-06-10T00:10
        [HttpGet("aggregate/{sensorKey}")]
        public ActionResult GetSensorEvent(string sensorKey, DateTimeOffset start, DateTimeOffset end)
        {
            AggregatedSensorEvent sensorEvent = _sensorEventStorage.GetEvent(sensorKey, start, end);
            if (sensorEvent is null)
            {
                return NotFound();
            }

            return Ok(sensorEvent);
        }

        // GET api/sensors/statistics
        [HttpGet("statistics")]
        public ActionResult GetAllSensorEvents()
        {
            IEnumerable<AggregatedSensorEvent> sensorEvents = _sensorEventStorage.GetAllEvents();

            return Ok(sensorEvents);
        }

        // POST api/sensors/subscribe
        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeMultiple(SensorSubscribeRequest request)
        {
            if (request.All)
            {
                await _sensorEventService.SubscribeAllAsync();

                return Ok("Subscribed to all sensors");
            }

            if (request.Sensors?.Length > 0)
            {
                await _sensorEventService.SubscribeAsync(request.Sensors);

                return Ok($"Subscribed to sensors [{string.Join(", ", request.Sensors)}]");
            }

            return BadRequest();
        }

        // POST api/sensors/subscribe/sensor_key
        [HttpPost("subscribe/{sensorKey}")]
        public async Task<IActionResult> SubscribeOne(string sensorKey)
        {
            await _sensorEventService.SubscribeAsync(new[] { sensorKey });

            return Ok($"Subscribed to {sensorKey}");
        }

        // POST api/sensors/unsubscribe
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> UnsubscribeMultiple(SensorUnsubscribeRequest request)
        {
            if (request.All)
            {
                await _sensorEventService.UnsubscribeAllAsync();
                
                return Ok("Unsubscribed from all sensors");
            }

            if (request.Sensors?.Length > 0)
            {
                await _sensorEventService.UnsubscribeAsync(request.Sensors);

                return Ok($"Unsubscribed from sensors [{string.Join(", ", request.Sensors)}]");
            }

            return BadRequest();
        }

        // POST api/sensors/unsubscribe/sensor_key
        [HttpPost("unsubscribe/{sensorKey}")]
        public async Task<IActionResult> UnsubscribeOne(string sensorKey)
        {
            await _sensorEventService.UnsubscribeAsync(new[] { sensorKey });

            return Ok($"Unsubscribed from {sensorKey}");
        }
    }
}

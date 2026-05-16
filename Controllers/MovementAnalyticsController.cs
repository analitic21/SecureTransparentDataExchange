using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services.Realtime;



namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementAnalyticsController : ControllerBase
    {
        private readonly MovementAnalyticsService _movementService;

        public MovementAnalyticsController(MovementAnalyticsService movementService)
        {
            _movementService = movementService;
        }

        [HttpPost("add-point")]
        public IActionResult AddPoint([FromBody] MovementPointRequest request)
        {
            _movementService.AddPoint(request.DeviceId, request.Lat, request.Lng);

            return Ok(new
            {
                message = "Point added successfully"
            });
        }

        [HttpGet("speed/{deviceId}")]
        public IActionResult GetSpeed(int deviceId)
        {
            var speed = _movementService.CalculateSpeed(deviceId);

            return Ok(new
            {
                deviceId,
                speedKmH = speed
            });
        }

        [HttpGet("history/{deviceId}")]
        public IActionResult GetHistory(int deviceId)
        {
            var history = _movementService.GetHistory(deviceId);

            return Ok(history.Select(p => new
            {
                latitude = p.lat,
                longitude = p.lng,
                timestamp = p.t
            }));
        }
    }

    public class MovementPointRequest
    {
        public int DeviceId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
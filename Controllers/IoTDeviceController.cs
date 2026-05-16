using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using  SecureTransparentDataExchange.DTOs;


namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IoTDeviceController : ControllerBase
    {
        private readonly IoTDeviceService _iotDeviceService;
        private readonly ILogger<IoTDeviceController> _logger;

        public IoTDeviceController(
            IoTDeviceService iotDeviceService,
            ILogger<IoTDeviceController> logger)
        {
            _iotDeviceService = iotDeviceService;
            _logger = logger;
        }

        // =========================
        // GET ALL DEVICES
        // GET: /api/iotdevice
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var devices = await _iotDeviceService.GetAllAsync();

                var result = devices.Select(d => new
                {
                    id = d.Id,
                    trackingNumber = d.TrackingNumber,
                    latitude = d.Latitude,
                    longitude = d.Longitude,
                    lastUpdated = d.LastUpdated,
                    isOnline = _iotDeviceService.IsDeviceOnline(d)
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading IoT devices.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
        // =========================
        // GET DEVICE BY ID
        // GET: /api/iotdevice/{id}
        // =========================
        [HttpGet("{id:int}/export")]
        public async Task<IActionResult> ExportRoute(int id)
        {
            var device = await _iotDeviceService.GetByIdAsync(id);
            if (device == null)
                return NotFound();

            var csv = $"Tracking,Latitude,Longitude,LastUpdated\n" +
                      $"{device.TrackingNumber},{device.Latitude},{device.Longitude},{device.LastUpdated}";

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

            return File(bytes, "text/csv", $"route_{id}.csv");
        }
        [HttpGet("{id:int}/route")]
        public async Task<IActionResult> GetRoute(int id)
        {
            var device = await _iotDeviceService.GetByIdAsync(id);
            if (device == null)
                return NotFound();

            return Ok(new[]
            {
        new {
            latitude = device.Latitude,
            longitude = device.Longitude,
            timestamp = device.LastUpdated
        }
    });
        }
        // =========================
        // GET DEVICE BY TRACKING NUMBER
        // GET: /api/iotdevice/tracking/{trackingNumber}
        // =========================
        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult<IoTDevice>> GetByTrackingNumber(string trackingNumber)
        {
            try
            {
                var device = await _iotDeviceService.GetByTrackingNumberAsync(trackingNumber);
                if (device == null)
                    return NotFound(new { message = $"Device with tracking number '{trackingNumber}' not found." });

                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading IoT device with tracking number {TrackingNumber}.", trackingNumber);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // =========================
        // CREATE DEVICE
        // POST: /api/iotdevice
        // =========================
        [HttpPost]
        public async Task<ActionResult<IoTDevice>> Create([FromBody] IoTDevice device)
        {
            if (device == null)
                return BadRequest(new { message = "Device data is required." });

            try
            {
                var created = await _iotDeviceService.CreateAsync(device);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating IoT device.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
        // =========================
        // GET DEVICE BY ID
        // GET: /api/iotdevice/{id}
        // =========================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IoTDevice>> GetById(int id)
        {
            try
            {
                var device = await _iotDeviceService.GetByIdAsync(id);
                if (device == null)
                    return NotFound(new { message = $"Device with ID {id} not found." });

                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading IoT device with ID {Id}.", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
        // =========================
        // UPDATE DEVICE
        // PUT: /api/iotdevice/{id}
        // =========================
        [HttpPut("{id:int}")]
        public async Task<ActionResult<IoTDevice>> Update(int id, [FromBody] IoTDevice updated)
        {
            if (updated == null)
                return BadRequest(new { message = "Updated device data is required." });

            try
            {
                var result = await _iotDeviceService.UpdateAsync(id, updated);
                if (result == null)
                    return NotFound(new { message = $"Device with ID {id} not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating IoT device with ID {Id}.", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // =========================
        // UPDATE TELEMETRY
        // POST: /api/iotdevice/telemetry
        // =========================
        [HttpPost("telemetry")]
        public async Task<IActionResult> UpdateTelemetry([FromBody] TelemetryDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Telemetry payload is required." });

            try
            {
                var ok = await _iotDeviceService.UpdateTelemetryAsync(dto.DeviceId, dto.Latitude, dto.Longitude);
                if (!ok)
                    return NotFound(new { message = $"Device with ID {dto.DeviceId} not found." });

                return Ok(new { message = "Telemetry updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating telemetry for device ID {Id}.", dto.DeviceId);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // =========================
        // HEARTBEAT
        // POST: /api/iotdevice/{id}/heartbeat
        // =========================
        [HttpPost("{id:int}/heartbeat")]
        public async Task<IActionResult> Heartbeat(int id)
        {
            try
            {
                var ok = await _iotDeviceService.HeartbeatAsync(id);
                if (!ok)
                    return NotFound(new { message = $"Device with ID {id} not found." });

                return Ok(new { message = "Heartbeat received." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending heartbeat for device ID {Id}.", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // =========================
        // GET LAST POSITION
        // GET: /api/iotdevice/{id}/position
        // =========================
        [HttpGet("{id:int}/position")]
        public async Task<IActionResult> GetLastPosition(int id)
        {
            try
            {
                var position = await _iotDeviceService.GetLastPositionAsync(id);
                if (position == null)
                    return NotFound(new { message = $"Device with ID {id} not found." });

                return Ok(new
                {
                    latitude = position.Value.lat,
                    longitude = position.Value.lng,
                    updatedAt = position.Value.updated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last position for device ID {Id}.", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // =========================
        // CHECK ONLINE STATUS
        // GET: /api/iotdevice/{id}/online
        // =========================
        [HttpGet("{id:int}/online")]
        public async Task<IActionResult> IsOnline(int id)
        {
            try
            {
                var device = await _iotDeviceService.GetByIdAsync(id);
                if (device == null)
                    return NotFound(new { message = $"Device with ID {id} not found." });

                var online = _iotDeviceService.IsDeviceOnline(device);

                return Ok(new
                {
                    deviceId = device.Id,
                    trackingNumber = device.TrackingNumber,
                    isOnline = online,
                    lastUpdated = device.LastUpdated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking online status for device ID {Id}.", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // =========================
        // DELETE BY ID
        // DELETE: /api/iotdevice/{id}
        // =========================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _iotDeviceService.DeleteAsync(id);
                if (!ok)
                    return NotFound(new { message = $"Device with ID {id} not found." });

                return Ok(new { message = "Device deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting IoT device with ID {Id}.", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // =========================
        // DELETE BY TRACKING NUMBER
        // DELETE: /api/iotdevice/tracking/{trackingNumber}
        // =========================
        [HttpDelete("tracking/{trackingNumber}")]
        public async Task<IActionResult> DeleteByTrackingNumber(string trackingNumber)
        {
            try
            {
                var ok = await _iotDeviceService.DeleteByTrackingNumberAsync(trackingNumber);
                if (!ok)
                    return NotFound(new { message = $"Device with tracking number '{trackingNumber}' not found." });

                return Ok(new { message = "Device deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting IoT device with tracking number {TrackingNumber}.", trackingNumber);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
    }
}   

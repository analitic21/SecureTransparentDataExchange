using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/geofence-security")]
    [Authorize(Roles = "Admin,Administrator,Employee")]
    public class GeoFenceSecurityController : ControllerBase
    {
        private readonly GeoFenceSecurityService _service;

        public GeoFenceSecurityController(GeoFenceSecurityService service)
        {
            _service = service;
        }

        [HttpPost("create-zone-with-key")]
        public async Task<IActionResult> CreateZoneWithKey([FromBody] CreateGeoFenceZoneRequest request)
        {
            var zone = await _service.CreateZoneWithKeyAsync(
                request.Name,
                request.CenterLat,
                request.CenterLng,
                request.RadiusMeters
            );

            return Ok(zone);
        }
    }

    public class CreateGeoFenceZoneRequest
    {
        public string Name { get; set; } = string.Empty;
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public int RadiusMeters { get; set; }
    }
}
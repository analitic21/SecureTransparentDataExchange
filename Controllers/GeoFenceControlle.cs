using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Services.Security;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeoFenceController : ControllerBase
    {
        private readonly GeoFenceService _geo;

        public GeoFenceController(GeoFenceService geo)
        {
            _geo = geo;
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var zones = await _geo.GetAllAsync();

            var result = zones.Select(z => new GeoFenceZoneResponseDto
            {
                Id = z.Id,
                Name = z.Name,
                CenterLat = z.CenterLat,
                CenterLng = z.CenterLng,
                RadiusMeters = z.RadiusMeters,
                CreatedAt = z.CreatedAt
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var zone = await _geo.GetByIdAsync(id);

            if (zone == null)
                return NotFound(new { message = "Zone not found" });

            return Ok(new GeoFenceZoneResponseDto
            {
                Id = zone.Id,
                Name = zone.Name,
                CenterLat = zone.CenterLat,
                CenterLng = zone.CenterLng,
                RadiusMeters = zone.RadiusMeters,
                CreatedAt = zone.CreatedAt
            });
        }

        [Authorize(Roles = "Admin,Administrator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GeoFenceZoneDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var zone = new GeoFenceZone
            {
                Name = dto.Name,
                CenterLat = dto.CenterLat,
                CenterLng = dto.CenterLng,
                RadiusMeters = dto.RadiusMeters,
                EncryptionKeyId = dto.EncryptionKeyId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _geo.CreateAsync(zone);

            return Ok(new GeoFenceZoneResponseDto
            {
                Id = created.Id,
                Name = created.Name,
                CenterLat = created.CenterLat,
                CenterLng = created.CenterLng,
                RadiusMeters = created.RadiusMeters,
                CreatedAt = created.CreatedAt
            });
        }

        [Authorize(Roles = "Admin,Administrator")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] GeoFenceZoneDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedZone = new GeoFenceZone
            {
                Name = dto.Name,
                CenterLat = dto.CenterLat,
                CenterLng = dto.CenterLng,
                RadiusMeters = dto.RadiusMeters,
                EncryptionKeyId = dto.EncryptionKeyId
            };

            var result = await _geo.UpdateAsync(id, updatedZone);

            if (result == null)
                return NotFound(new { message = "Zone not found" });

            return Ok(new GeoFenceZoneResponseDto
            {
                Id = result.Id,
                Name = result.Name,
                CenterLat = result.CenterLat,
                CenterLng = result.CenterLng,
                RadiusMeters = result.RadiusMeters,
                CreatedAt = result.CreatedAt
            });
        }

        [Authorize(Roles = "Admin,Administrator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _geo.DeleteAsync(id);
            return Ok(new { message = "Zone removed" });
        }

        // For employees and admins
        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpGet("check/internal")]
        public async Task<IActionResult> CheckInternal(double lat, double lng)
        {
            var zones = await _geo.GetAllAsync();

            foreach (var zone in zones)
            {
                if (_geo.IsInsideZone(zone, lat, lng))
                {
                    return Ok(new GeoFenceZoneResponseDto
                    {
                        Id = zone.Id,
                        Name = zone.Name,
                        CenterLat = zone.CenterLat,
                        CenterLng = zone.CenterLng,
                        RadiusMeters = zone.RadiusMeters,
                        CreatedAt = zone.CreatedAt
                    });
                }
            }

            return Ok(null);
        }

        // Для клиента — без координат, радиуса и ключей
        [Authorize(Roles = "User,Business")]
        [HttpGet("check/client")]
        public async Task<IActionResult> CheckClient(double lat, double lng)
        {
            var zones = await _geo.GetAllAsync();

            foreach (var zone in zones)
            {
                if (_geo.IsInsideZone(zone, lat, lng))
                {
                    return Ok(new GeoFenceClientCheckDto
                    {
                        Inside = true,
                        ZoneName = zone.Name
                    });
                }
            }

            return Ok(new GeoFenceClientCheckDto
            {
                Inside = false,
                ZoneName = null
            });
        }
    }
}
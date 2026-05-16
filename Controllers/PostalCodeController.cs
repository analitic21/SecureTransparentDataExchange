using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models.Location;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/postalcode")]
    public class PostalCodeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostalCodeController> _logger;

        public PostalCodeController(
            ApplicationDbContext context,
            ILogger<PostalCodeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================
        // GET /api/postalcode
        // =====================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<PostalCodeDTO>>> GetAll()
        {
            try
            {
                var postalCodes = await _context.PostalCodes
                    .AsNoTracking()
                    .OrderBy(p => p.Code)
                    .Select(p => new PostalCodeDTO
                    {
                        Id = p.Id,
                        Code = p.Code ?? ""
                    })
                    .ToListAsync();

                return Ok(postalCodes);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading postal codes");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =====================================================
        // GET /api/postalcode/{id}
        // =====================================================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostalCodeDTO>> GetById(int id)
        {
            try
            {
                var pc = await _context.PostalCodes
                    .AsNoTracking()
                    .Where(p => p.Id == id)
                    .Select(p => new PostalCodeDTO
                    {
                        Id = p.Id,
                        Code = p.Code ?? ""
                    })
                    .FirstOrDefaultAsync();

                if (pc == null)
                    return NotFound(new { message = "Postal code not found" });

                return Ok(pc);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading postal code by id");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =====================================================
        // GET /api/postalcode/city/{cityId}
        // =====================================================
        [HttpGet("city/{cityId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PostalCodeDTO>>> GetByCity(int cityId)
        {
            try
            {
                var postalCodes = await _context.PostalCodes
                    .AsNoTracking()
                    .Where(p => p.CityId == cityId)
                    .OrderBy(p => p.Code)
                    .Select(p => new PostalCodeDTO
                    {
                        Id = p.Id,
                        Code = p.Code ?? ""
                    })
                    .ToListAsync();

                return Ok(postalCodes);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading postal codes by city");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =====================================================
        // POST /api/postalcode
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PostalCodeDTO>> Create([FromBody] PostalCodeCreateDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { message = "Postal code is required" });

            try
            {
                var code = dto.Code.Trim();

                bool cityExists = await _context.Cities.AnyAsync(c => c.Id == dto.CityId);
                if (!cityExists)
                    return BadRequest(new { message = "City not found" });

                bool exists = await _context.PostalCodes
                    .AnyAsync(p => p.Code == code && p.CityId == dto.CityId);

                if (exists)
                    return Conflict(new { message = "Postal code already exists for this city" });

                var postalCode = new PostalCode
                {
                    Code = code,
                    CityId = dto.CityId
                };

                _context.PostalCodes.Add(postalCode);
                await _context.SaveChangesAsync();

                return Ok(new PostalCodeDTO
                {
                    Id = postalCode.Id,
                    Code = postalCode.Code ?? ""
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating postal code");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =====================================================
        // PUT /api/postalcode/{id}
        // =====================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PostalCodeCreateDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { message = "Postal code is required" });

            try
            {
                var postalCode = await _context.PostalCodes.FindAsync(id);
                if (postalCode == null)
                    return NotFound(new { message = "Postal code not found" });

                bool cityExists = await _context.Cities.AnyAsync(c => c.Id == dto.CityId);
                if (!cityExists)
                    return BadRequest(new { message = "City not found" });

                var code = dto.Code.Trim();

                bool exists = await _context.PostalCodes
                    .AnyAsync(p => p.Id != id && p.Code == code && p.CityId == dto.CityId);

                if (exists)
                    return Conflict(new { message = "Postal code already exists for this city" });

                postalCode.Code = code;
                postalCode.CityId = dto.CityId;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Postal code updated" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating postal code");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =====================================================
        // DELETE /api/postalcode/{id}
        // =====================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var postalCode = await _context.PostalCodes.FindAsync(id);
                if (postalCode == null)
                    return NotFound(new { message = "Postal code not found" });

                bool used = await _context.Addresses
                    .AnyAsync(a => a.PostalCodeId == id);

                if (used)
                    return Conflict(new { message = "Postal code is used by addresses" });

                _context.PostalCodes.Remove(postalCode);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Postal code deleted" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting postal code");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
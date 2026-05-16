using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/country")]
    public class CountryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CountryController> _logger;

        public CountryController(
            ApplicationDbContext context,
            ILogger<CountryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================
        // GET /api/country
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<CountryDTO>>> GetAll()
        {
            try
            {
                var countries = await _context.Countries
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new CountryDTO
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToListAsync();

                return Ok(countries);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading countries");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =========================
        // GET /api/country/{id}
        // =========================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<CountryDTO>> GetById(int id)
        {
            try
            {
                var country = await _context.Countries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (country == null)
                    return NotFound(new { message = "Country not found" });

                return Ok(new CountryDTO
                {
                    Id = country.Id,
                    Name = country.Name
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading country by id");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =========================
        // POST /api/country
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CountryCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Country name is required" });

            try
            {
                var name = dto.Name.Trim();

                bool exists = await _context.Countries
                    .AnyAsync(c => c.Name.ToLower() == name.ToLower());

                if (exists)
                    return Conflict(new { message = "Country already exists" });

                var country = new Country
                {
                    Name = name
                };

                _context.Countries.Add(country);
                await _context.SaveChangesAsync();

                return Ok(new CountryDTO
                {
                    Id = country.Id,
                    Name = country.Name
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating country");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =========================
        // PUT /api/country/{id}
        // =========================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CountryCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Country name is required" });

            try
            {
                var country = await _context.Countries.FindAsync(id);
                if (country == null)
                    return NotFound(new { message = "Country not found" });

                var name = dto.Name.Trim();

                bool nameTaken = await _context.Countries
                    .AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());

                if (nameTaken)
                    return Conflict(new { message = "Country with this name already exists" });

                country.Name = name;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Country updated" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating country");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =========================
        // DELETE /api/country/{id}
        // =========================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var country = await _context.Countries
                    .Include(c => c.Cities)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (country == null)
                    return NotFound(new { message = "Country not found" });

                if (country.Cities != null && country.Cities.Any())
                    return BadRequest(new { message = "Country cannot be deleted — cities exist" });

                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Country deleted" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting country");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
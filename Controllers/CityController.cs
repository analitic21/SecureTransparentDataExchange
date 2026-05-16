using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models.Location;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/city")]
    public class CityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CityController> _logger;

        public CityController(ApplicationDbContext context, ILogger<CityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===========================
        // GET ALL
        // ===========================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var cities = await _context.Cities
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new CityDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CountryId = c.CountryId
                    })
                    .ToListAsync();

                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cities");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===========================
        // GET BY ID
        // ===========================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var city = await _context.Cities
                    .AsNoTracking()
                    .Where(c => c.Id == id)
                    .Select(c => new CityDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CountryId = c.CountryId
                    })
                    .FirstOrDefaultAsync();

                if (city == null)
                    return NotFound(new { message = "City not found" });

                return Ok(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading city by id");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===========================
        // GET BY COUNTRY
        // ===========================
        [HttpGet("country/{countryId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCountry(int countryId)
        {
            try
            {
                var cities = await _context.Cities
                    .AsNoTracking()
                    .Where(c => c.CountryId == countryId)
                    .OrderBy(c => c.Name)
                    .Select(c => new CityDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CountryId = c.CountryId
                    })
                    .ToListAsync();

                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cities by country");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // ===========================
        // CREATE
        // ===========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CityCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "City name is required" });

            bool countryExists = await _context.Countries
                .AnyAsync(c => c.Id == dto.CountryId);

            if (!countryExists)
                return BadRequest(new { message = "Invalid CountryId" });

            bool exists = await _context.Cities.AnyAsync(c =>
                c.Name.ToLower() == dto.Name.ToLower() &&
                c.CountryId == dto.CountryId);

            if (exists)
                return Conflict(new { message = "City already exists in this country" });

            var city = new City
            {
                Name = dto.Name.Trim(),
                CountryId = dto.CountryId
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return Ok(new { message = "City created", id = city.Id });
        }

        // ===========================
        // UPDATE
        // ===========================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CityCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "City name is required" });

            bool countryExists = await _context.Countries
                .AnyAsync(c => c.Id == dto.CountryId);  

            if (!countryExists)
                return BadRequest(new { message = "Invalid CountryId" });

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound(new { message = "City not found" });

            bool duplicateExists = await _context.Cities.AnyAsync(c =>
                c.Id != id &&
                c.Name.ToLower() == dto.Name.ToLower() &&
                c.CountryId == dto.CountryId);

            if (duplicateExists)
                return Conflict(new { message = "City already exists in this country" });

            city.Name = dto.Name.Trim();
            city.CountryId = dto.CountryId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "City updated" });
        }
        // ===========================
        // DELETE
        // ===========================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound(new { message = "City not found" });

            bool hasPostalCodes = await _context.PostalCodes
                .AnyAsync(p => p.CityId == id);

            if (hasPostalCodes)
                return BadRequest(new { message = "City cannot be deleted — linked postal codes exist" });

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return Ok(new { message = "City deleted" });
        }
    }
}
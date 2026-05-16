using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models.Location;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/address")]
    [AllowAnonymous]
    public class AddressController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddressController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await Query().ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var address = await Query().FirstOrDefaultAsync(a => a.Id == id);
            return address == null ? NotFound() : Ok(address);
        }

        [HttpGet("postalcode/{postalCodeId:int}")]
        public async Task<IActionResult> GetByPostalCode(int postalCodeId)
        {
            return Ok(await Query()
                .Where(a => a.PostalCodeId == postalCodeId)
                .ToListAsync());
        }

        [HttpGet("city/{cityId:int}")]
        public async Task<IActionResult> GetByCity(int cityId)
        {
            return Ok(await Query()
                .Where(a => a.CityId == cityId)
                .ToListAsync());
        }

        [HttpGet("country/{countryId:int}")]
        public async Task<IActionResult> GetByCountry(int countryId)
        {
            return Ok(await Query()
                .Where(a => a.CountryId == countryId)
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddressCreateDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid payload.");

            if (string.IsNullOrWhiteSpace(dto.Street))
                return BadRequest("Street is required.");

            var postalCode = await _context.PostalCodes
                .Include(p => p.City)
                .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(p => p.Id == dto.PostalCodeId);

            if (postalCode == null)
                return BadRequest("Postal code not found.");

            var exists = await _context.Addresses.AnyAsync(a =>
                a.Street == dto.Street &&
                a.PostalCodeId == dto.PostalCodeId);

            if (exists)
                return BadRequest("Address already exists.");

            var address = new Address
            {
                Street = dto.Street,
                PostalCodeId = dto.PostalCodeId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = address.Id },
                await Query().FirstAsync(a => a.Id == address.Id));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AddressCreateDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid payload.");

            if (string.IsNullOrWhiteSpace(dto.Street))
                return BadRequest("Street is required.");

            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
                return NotFound();

            var postalCodeExists = await _context.PostalCodes.AnyAsync(p => p.Id == dto.PostalCodeId);
            if (!postalCodeExists)
                return BadRequest("Postal code not found.");

            address.Street = dto.Street;
            address.PostalCodeId = dto.PostalCodeId;
            address.Latitude = dto.Latitude;
            address.Longitude = dto.Longitude;

            await _context.SaveChangesAsync();

            return Ok(await Query().FirstAsync(a => a.Id == id));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
                return NotFound();

            var usedByLogin = await _context.Users.AnyAsync(u => u.AddressId == id);
            var usedByClient = await _context.Clients.AnyAsync(c => c.AddressId == id);
            var usedByCompany = await _context.Companies.AnyAsync(c => c.AddressId == id);

            if (usedByLogin || usedByClient || usedByCompany)
                return BadRequest("Address cannot be deleted because it is used.");

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private IQueryable<AddressDTO> Query()
        {
            return _context.Addresses
                .AsNoTracking()
                .OrderBy(a => a.Id)
                .Select(a => new AddressDTO
                {
                    Id = a.Id,
                    Street = a.Street,
                    PostalCodeId = a.PostalCodeId,
                    PostalCode = a.PostalCode.Code,
                    CityId = a.PostalCode.CityId,
                    CountryId = a.PostalCode.City.CountryId,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude
                });
        }
    }
}
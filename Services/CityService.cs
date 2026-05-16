using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models.Location;

namespace SecureTransparentDataExchange.Services
{
    public class CityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CityService> _logger;

        public CityService(ApplicationDbContext context, ILogger<CityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private IQueryable<CityDTO> BaseQuery()
        {
            return _context.Cities
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CityDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    CountryId = c.CountryId
                });
        }

        public async Task<List<CityDTO>> GetAllAsync()
        {
            return await BaseQuery().ToListAsync();
        }

        public async Task<CityDTO?> GetByIdAsync(int id)
        {
            return await BaseQuery()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CityDTO> CreateAsync(CityDTO model)
        {
            var exists = await _context.Cities.AnyAsync(c =>
                c.Name.ToLower() == model.Name.ToLower() &&
                c.CountryId == model.CountryId);

            if (exists)
                throw new InvalidOperationException("City already exists in this country");

            var countryExists = await _context.Countries.AnyAsync(c => c.Id == model.CountryId);
            if (!countryExists)
                throw new InvalidOperationException("Country not found");

            var city = new City
            {
                Name = model.Name,
                CountryId = model.CountryId
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            model.Id = city.Id;
            return model;
        }

        public async Task<CityDTO?> UpdateAsync(int id, CityDTO model)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return null;

            var countryExists = await _context.Countries.AnyAsync(c => c.Id == model.CountryId);
            if (!countryExists)
                throw new InvalidOperationException("Country not found");

            city.Name = model.Name;
            city.CountryId = model.CountryId;

            await _context.SaveChangesAsync();

            model.Id = city.Id;
            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return false;

            bool hasPostalCodes = await _context.PostalCodes.AnyAsync(p => p.CityId == id);

            if (hasPostalCodes)
                throw new InvalidOperationException("City cannot be deleted because postal codes exist.");

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
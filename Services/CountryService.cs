using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.Location;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SecureTransparentDataExchange.Services
{
    public class CountryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CountryService> _logger;

        public CountryService(ApplicationDbContext context, ILogger<CountryService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ===========================
        // GET ALL COUNTRIES
        // ===========================
        public async Task<List<Country>> GetAllCountriesAsync()
        {
            try
            {
                _logger.LogInformation("Loading all countries...");

                return await _context.Countries
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading countries");
                throw;
            }
        }

        // ===========================
        // GET SINGLE COUNTRY
        // ===========================
        public async Task<Country?> GetCountryByIdAsync(int id)
        {
            return await _context.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // ===========================
        // ADD COUNTRY
        // ===========================
        public async Task<Country> AddCountryAsync(string name)
        {
            var existing = await _context.Countries
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

            if (existing != null)
                throw new InvalidOperationException("Country already exists");

            var country = new Country { Name = name };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return country;
        }

        // ===========================
        // DELETE COUNTRY (IF NO CITIES)
        // ===========================
        public async Task<bool> DeleteCountryAsync(int id)
        {
            var country = await _context.Countries
                .Include(c => c.Cities)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (country == null)
                throw new KeyNotFoundException("Country not found");

            if (country.Cities != null && country.Cities.Count > 0)
                throw new InvalidOperationException("Cannot delete: country has cities assigned");

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

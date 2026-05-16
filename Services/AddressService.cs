using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using  SecureTransparentDataExchange.DTOs;

namespace SecureTransparentDataExchange.Services
{
    public class AddressService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AddressService> _logger;

        public AddressService(ApplicationDbContext context, ILogger<AddressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private IQueryable<AddressDTO> BaseQuery()
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
                    City = a.PostalCode.City.Name,

                    CountryId = a.PostalCode.City.CountryId,
                    Country = a.PostalCode.City.Country.Name,

                    Latitude = a.Latitude,
                    Longitude = a.Longitude
                });
        }

        public async Task<List<AddressDTO>> GetByPostalCodeIdAsync(int postalCodeId)
        {
            return await BaseQuery()
                .Where(a => a.PostalCodeId == postalCodeId)
                .ToListAsync();
        }

        public async Task<List<AddressDTO>> GetByCityIdAsync(int cityId)
        {
            return await BaseQuery()
                .Where(a => a.CityId == cityId)
                .ToListAsync();
        }

        public async Task<List<AddressDTO>> GetByCountryIdAsync(int countryId)
        {
            return await BaseQuery()
                .Where(a => a.CountryId == countryId)
                .ToListAsync();
        }

        public async Task<List<AddressDTO>> GetAllAsync()
        {
            return await BaseQuery().ToListAsync();
        }
    }
}
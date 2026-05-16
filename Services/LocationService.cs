using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models.Location;


namespace SecureTransparentDataExchange.Services
{
    public class LocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LocationService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddCountryAsync(Country country)
        {
            ArgumentNullException.ThrowIfNull(country);

            bool exists = await _context.Countries
                .AnyAsync(c => c.Name == country.Name);

            if (exists)
                throw new InvalidOperationException("Country already exists.");

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
        }

        public async Task AddCityAsync(City city)
        {
            ArgumentNullException.ThrowIfNull(city);

            bool exists = await _context.Cities
                .AnyAsync(c => c.Name == city.Name && c.CountryId == city.CountryId);

            if (exists)
                throw new InvalidOperationException("City already exists.");

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
        }

        public async Task AddPostalCodeAsync(PostalCode postal)
        {
            ArgumentNullException.ThrowIfNull(postal);

            bool exists = await _context.PostalCodes
                .AnyAsync(p => p.Code == postal.Code && p.CityId == postal.CityId);

            if (exists)
                throw new InvalidOperationException("Postal code already exists.");

            _context.PostalCodes.Add(postal);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CountryDTO>> GetAllCountriesAsync()
        {
            var countries = await _context.Countries.ToListAsync();
            return _mapper.Map<IEnumerable<CountryDTO>>(countries);
        }

        public async Task<IEnumerable<CityDTO>> GetAllCitiesAsync()
        {
            var cities = await _context.Cities
                .Include(c => c.Country)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CityDTO>>(cities);
        }

        public async Task<IEnumerable<PostalCodeDTO>> GetAllPostalCodesAsync()
        {
            var postalCodes = await _context.PostalCodes
                .Include(p => p.City)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PostalCodeDTO>>(postalCodes);
        }

        public async Task<List<LocationTreeDTO>> GetFullLocationTreeAsync()
        {
            var countries = await _context.Countries
                .Include(c => c.Cities)
                    .ThenInclude(city => city.PostalCodes)
                        .ThenInclude(pc => pc.Address)
                .ToListAsync();

            return countries.Select(country => new LocationTreeDTO
            {
                CountryId = country.Id,
                CountryName = country.Name,

                Cities = country.Cities.Select(city => new CityNodeDTO
                {
                    CityId = city.Id,
                    CityName = city.Name,

                    PostalCodes = city.PostalCodes.Select(pc => new PostalNodeDTO
                    {
                        PostalCodeId = pc.Id,
                        Code = pc.Code,

                        Addresses = pc.Address.Select(a => new AddressNodeDTO
                        {
                            AddressId = a.Id,
                            Street = a.Street
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();
        }
    }
}
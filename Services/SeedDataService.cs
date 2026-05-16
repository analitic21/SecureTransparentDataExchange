using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Location;

namespace SecureTransparentDataExchange.Services
{
    public class SeedDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(ApplicationDbContext context, ILogger<SeedDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private sealed record LocationSeed(string Country, string City, string PostalCode, string Street);

        public async Task InitializeDataAsync()
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var data = new List<LocationSeed>
                {
                    new("USA", "New York", "10001", "5th Avenue"),
                    new("USA", "New York", "10002", "Broadway"),
                    new("USA", "Los Angeles", "90001", "Sunset Boulevard"),
                    new("USA", "Los Angeles", "90002", "Hollywood Boulevard"),

                    new("Canada", "Toronto", "M5H 2N2", "King Street"),
                    new("Canada", "Toronto", "M5V 2T6", "Queen Street"),
                    new("Canada", "Vancouver", "V6B 1H4", "Granville Street"),

                    new("Germany", "Berlin", "10115", "Friedrichstraße"),
                    new("Germany", "Berlin", "10117", "Unter den Linden"),
                    new("Germany", "Munich", "80331", "Marienplatz"),

                    new("Latvia", "Riga", "LV-1010", "Brīvības iela"),
                    new("Latvia", "Riga", "LV-1050", "Krišjāņa Barona iela"),
                    new("Latvia", "Daugavpils", "LV-5401", "Rīgas iela"),

                    new("Lithuania", "Vilnius", "LT-01100", "Gedimino prospektas"),
                    new("Lithuania", "Kaunas", "LT-44280", "Laisvės alėja"),

                    new("Estonia", "Tallinn", "10111", "Narva maantee"),
                    new("Estonia", "Tartu", "51003", "Riia tänav"),

                    new("United Kingdom", "London", "SW1A 1AA", "The Mall"),
                    new("United Kingdom", "London", "EC1A 1BB", "Fleet Street"),
                    new("United Kingdom", "Manchester", "M1 1AE", "Oxford Road"),

                    new("France", "Paris", "75001", "Rue de Rivoli"),
                    new("France", "Paris", "75008", "Champs-Élysées"),
                    new("France", "Lyon", "69001", "Rue de la République"),

                    new("Spain", "Madrid", "28001", "Calle de Alcalá"),
                    new("Spain", "Barcelona", "08001", "La Rambla"),

                    new("Italy", "Rome", "00100", "Via del Corso"),
                    new("Italy", "Milan", "20121", "Corso Buenos Aires")
                };

                var countryNames = data.Select(d => d.Country).Distinct().ToList();

                var existingCountries = await _context.Countries
                    .Where(c => countryNames.Contains(c.Name))
                    .ToListAsync();

                var countryDict = existingCountries.ToDictionary(c => c.Name, c => c);

                foreach (var countryName in countryNames)
                {
                    if (!countryDict.ContainsKey(countryName))
                    {
                        var country = new Country { Name = countryName };
                        _context.Countries.Add(country);
                        countryDict[countryName] = country;
                    }
                }

                await _context.SaveChangesAsync();

                var cityGroups = data
                    .Select(d => new { d.Country, d.City })
                    .Distinct()
                    .ToList();

                var existingCities = await _context.Cities
                    .Include(c => c.Country)
                    .ToListAsync();

                var cityDict = existingCities
                    .Where(c => c.Country != null)
                    .ToDictionary(c => (c.Country.Name, c.Name), c => c);

                foreach (var item in cityGroups)
                {
                    var key = (item.Country, item.City);

                    if (!cityDict.ContainsKey(key))
                    {
                        var city = new City
                        {
                            Name = item.City,
                            CountryId = countryDict[item.Country].Id
                        };

                        _context.Cities.Add(city);
                        cityDict[key] = city;
                    }
                }

                await _context.SaveChangesAsync();

                var existingPostalCodes = await _context.PostalCodes.ToListAsync();

                var postalDict = existingPostalCodes
                    .ToDictionary(p => (p.CityId, p.Code), p => p);

                foreach (var item in data)
                {
                    var city = cityDict[(item.Country, item.City)];
                    var key = (city.Id, item.PostalCode);

                    if (!postalDict.ContainsKey(key))
                    {
                        var postalCode = new PostalCode
                        {
                            Code = item.PostalCode,
                            CityId = city.Id
                        };

                        _context.PostalCodes.Add(postalCode);
                        postalDict[key] = postalCode;
                    }
                }

                await _context.SaveChangesAsync();

                var existingAddresses = await _context.Addresses.ToListAsync();

                var addressDict = existingAddresses
                    .ToDictionary(a => (a.Street, a.PostalCodeId), a => a);

                foreach (var item in data)
                {
                    var city = cityDict[(item.Country, item.City)];
                    var postalCode = postalDict[(city.Id, item.PostalCode)];

                    var key = (item.Street, postalCode.Id);

                    if (!addressDict.ContainsKey(key))
                    {
                        var address = new Address
                        {
                            Street = item.Street,
                            PostalCodeId = postalCode.Id
                        };

                        _context.Addresses.Add(address);
                        addressDict[key] = address;
                    }
                }

                await _context.SaveChangesAsync();

                var hasActiveKey = await _context.EncryptionKeys
                    .AnyAsync(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow);

                if (!hasActiveKey)
                {
                    _context.EncryptionKeys.Add(new EncryptionKey
                    {
                        PublicKey = Guid.NewGuid().ToString("N"),
                        PrivateKey = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = null
                    });

                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();

                _logger.LogInformation("Global location data seeded successfully.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Seeding error");
                throw;
            }
        }

        public Task InitializeData() => InitializeDataAsync();
    }
}
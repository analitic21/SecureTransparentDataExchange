using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Location;

namespace SecureTransparentDataExchange.Services
{
    public class ClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientService> _logger;
        private readonly DeliveryPriceService _deliveryPriceService;

        public ClientService(
            ApplicationDbContext context,
            ILogger<ClientService> logger,
            DeliveryPriceService deliveryPriceService)
        {
            _context = context;
            _logger = logger;
            _deliveryPriceService = deliveryPriceService;
        }

        public async Task<(bool success, string message, List<City> cities)> GetAllCitiesAsync()
        {
            try
            {
                var cities = await _context.Cities
                    .Include(c => c.Country)
                    .Include(c => c.PostalCodes)
                    .ToListAsync();

                return (true, "Cities retrieved successfully.", cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cities.");
                return (false, ex.Message, new List<City>());
            }
        }

        public async Task<(bool success, string message, List<Country> countries)> GetAllCountriesAsync()
        {
            try
            {
                var countries = await _context.Countries.ToListAsync();
                return (true, "Countries retrieved successfully.", countries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching countries.");
                return (false, ex.Message, new List<Country>());
            }
        }

        public async Task<(bool success, string message, List<PostalCode> postalCodes)> GetAllPostalCodesAsync()
        {
            try
            {
                var postalCodes = await _context.PostalCodes
                    .Include(p => p.City)
                    .ToListAsync();

                return (true, "Postal codes retrieved successfully.", postalCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching postal codes.");
                return (false, ex.Message, new List<PostalCode>());
            }
        }

        public async Task<(bool success, string message, List<Address> addresses)> GetAllAddressesAsync()
        {
            try
            {
                var addresses = await _context.Addresses
                    .Include(a => a.PostalCode)
                        .ThenInclude(p => p.City)
                            .ThenInclude(c => c.Country)
                    .ToListAsync();

                return (true, "Addresses retrieved successfully.", addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses.");
                return (false, ex.Message, new List<Address>());
            }
        }

        public async Task<Client?> GetClientByLoginIdAsync(int loginId)
        {
            return await _context.Clients
                .Include(c => c.Address)
                    .ThenInclude(a => a!.PostalCode)
                        .ThenInclude(p => p.City)
                            .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(c => c.LoginId == loginId);
        }

        public async Task<Client> GetOrCreateClientAsync(int loginId, string name, string lastName)
        {
            var client = await GetClientByLoginIdAsync(loginId);
            if (client != null)
                return client;

            client = new Client
            {
                LoginId = loginId,
                Name = name,
                LastName = lastName
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client;
        }

        public async Task<Client?> GetClientByIdAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.Address)
                    .ThenInclude(a => a!.PostalCode)
                        .ThenInclude(p => p.City)
                            .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            if (client.LoginId <= 0)
                throw new InvalidOperationException("LoginId is required.");

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task UpdateClientAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return false;

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Client>> GetAllClientsAsync()
        {
            return await _context.Clients
                .Include(c => c.Address)
                    .ThenInclude(a => a!.PostalCode)
                        .ThenInclude(p => p.City)
                            .ThenInclude(c => c.Country)
                .ToListAsync();
        }

        public async Task<DeliveryPriceResponse> GetClientDeliveryPriceAsync(
            int clientId,
            int warehouseAddressId)
        {
            var client = await GetClientByIdAsync(clientId)
                ?? throw new Exception("Client not found.");

            if (client.AddressId == null)
                throw new Exception("Client does not have an associated address.");

            return await _deliveryPriceService.CalculateDeliveryPriceAsync(
                warehouseAddressId,
                client.AddressId.Value);
        }
    }
}
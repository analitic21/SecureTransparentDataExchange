using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Location;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ClientService _clientService;
        private readonly DeliveryPriceService _deliveryPriceService;
        private readonly ILogger<ClientController> _logger;
        private readonly ApplicationDbContext _context;

        public ClientController(
            ClientService clientService,
            DeliveryPriceService deliveryPriceService,
            ILogger<ClientController> logger,
            ApplicationDbContext context)
        {
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _deliveryPriceService = deliveryPriceService ?? throw new ArgumentNullException(nameof(deliveryPriceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ========================= GET COUNTRIES =========================
        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            try
            {
                var result = await _clientService.GetAllCountriesAsync();
                if (!result.success)
                    return StatusCode(500, new { message = result.message });

                return Ok(result.countries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching countries");
                return StatusCode(500, new { message = "Internal server error while fetching countries." });
            }
        }

        // ========================= GET CITIES =========================
        [HttpGet("cities")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            try
            {
                var result = await _clientService.GetAllCitiesAsync();
                if (!result.success)
                    return StatusCode(500, new { message = result.message });

                return Ok(result.cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cities");
                return StatusCode(500, new { message = "Internal server error while fetching cities." });
            }
        }

        // ========================= GET POSTAL CODES =========================
        [HttpGet("postal-codes")]
        public async Task<ActionResult<IEnumerable<PostalCode>>> GetPostalCodes()
        {
            try
            {
                var result = await _clientService.GetAllPostalCodesAsync();
                if (!result.success)
                    return StatusCode(500, new { message = result.message });

                return Ok(result.postalCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching postal codes");
                return StatusCode(500, new { message = "Internal server error while fetching postal codes." });
            }
        }

        // ========================= GET ADDRESSES =========================
        [HttpGet("addresses")]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            try
            {
                var result = await _clientService.GetAllAddressesAsync();
                if (!result.success)
                    return StatusCode(500, new { message = result.message });

                return Ok(result.addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses");
                return StatusCode(500, new { message = "Internal server error while fetching addresses." });
            }
        }

        // ========================= GET CLIENT BY ID (LEFT, BUT PROTECTED) =========================
        // To prevent regular users from viewing other people's clients, the endpoint requires Admin.
        // If you need it differently, tell us, we'll change it to "me"
        [Authorize(Roles = "Admin,Administrator")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClientById(int id)
        {
            try
            {
                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null)
                    return NotFound(new { message = "Client not found." });

                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching client with ID {id}");
                return StatusCode(500, new { message = "Internal server error while fetching client." });
            }
        }

        // ========================= GET MY CLIENT =========================
        // Returns the client profile of the current user (by JWT).
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyClient()
        {
            try
            {
                var userId = GetUserIdFromJwt();
                if (userId == null) return Unauthorized(new { message = "UserId claim missing." });

                var client = await _clientService.GetClientByLoginIdAsync(userId.Value);

                if (client == null)
                    return NotFound(new { message = "Client profile not found for this user." });

                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user's client");
                return StatusCode(500, new { message = "Internal server error while fetching your client profile." });
            }
        }
        // ========================= CREATE OR UPDATE MY CLIENT =========================
        // IMPORTANT: user does NOT pass a LoginId. We get it from the JWT.
        // If the client already exists, we update it; if not, we create it.
        [Authorize]
        [HttpPost("me")]
        public async Task<IActionResult> CreateOrUpdateMyClient([FromBody] Client client)
        {
            if (client == null)
                return BadRequest(new { message = "Client payload is required." });

            try
            {
                var userId = GetUserIdFromJwt();
                if (userId == null)
                    return Unauthorized(new { message = "UserId claim missing." });

                client.LoginId = userId.Value;

                var existing = await _clientService.GetClientByLoginIdAsync(userId.Value);

                if (existing == null)
                {
                    var created = await _clientService.CreateClientAsync(client);
                    return Ok(new { message = "Client profile created.", client = created });
                }

                client.Id = existing.Id;

                await _clientService.UpdateClientAsync(client);
                var updated = await _clientService.GetClientByIdAsync(existing.Id);

                return Ok(new { message = "Client profile updated.", client = updated ?? client });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating client profile");
                return StatusCode(500, new { message = ex.Message });
            }
        }



        // ========================= CREATE CLIENT (ADMIN ONLY) =========================
        // We leave the old endpoint, but restrict it to admin.
        [Authorize(Roles = "Admin,Administrator")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            try
            {
                var createdClient = await _clientService.CreateClientAsync(client);
                return CreatedAtAction(nameof(GetClientById), new { id = createdClient.Id }, createdClient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ========================= DELETE CLIENT (ADMIN ONLY) =========================
        [Authorize(Roles = "Admin,Administrator")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                var success = await _clientService.DeleteClientAsync(id);
                if (!success)
                    return NotFound(new { message = "Client not found." });

                return Ok(new { success = true, message = "Client deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client");
                return StatusCode(500, new { message = "Internal server error while deleting client." });
            }
        }

        // ========================= UPDATE CLIENT (ADMIN ONLY) =========================
        [Authorize(Roles = "Admin,Administrator")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateClient([FromBody] Client client)
        {
            try
            {
                await _clientService.UpdateClientAsync(client);
                return Ok(new { success = true, message = "Client updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client");
                return StatusCode(500, new { message = "Internal server error while updating client." });
            }
        }

        // ========================= DELIVERY PRICE =========================
        // The admin can count by any id, the user can only count by his own.
        [Authorize]
        [HttpGet("{id}/delivery-price")]
        public async Task<IActionResult> GetClientDeliveryPrice(int id)
        {
            try
            {
                // If you are not an admin, we only allow our client
                var isAdmin = User.IsInRole("Admin") ||
                               User.IsInRole("Administrator");


                if (!isAdmin)
                {
                    var userId = GetUserIdFromJwt();
                    if (userId == null) return Unauthorized(new { message = "UserId claim missing." });

                    var myClient = await _clientService.GetClientByLoginIdAsync(userId.Value);
                    if (myClient == null)
                        return NotFound(new { message = "Client profile not found for this user." });

                    if (myClient.Id != id)
                        return Forbid();
                }

                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null)
                    return NotFound(new { message = "Client not found." });

                if (client.AddressId == null)
                    return BadRequest(new { message = "Client does not have an associated address." });

                int warehouseAddressId = 1; // TODO: move to config/table
                var result = await _deliveryPriceService.CalculateDeliveryPriceAsync(
                    warehouseAddressId,
                    client.AddressId.Value
                );

                if (!result.Success)
                    return StatusCode(500, new { message = result.Message });

                return Ok(new
                {
                    price = result.Price,
                    estimatedDeliveryTime = result.EstimatedDeliveryTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating delivery price for client {id}");
                return StatusCode(500, new { message = "Internal server error while calculating delivery price." });
            }
        }
        // ========================= GET ALL CLIENTS (ADMIN ONLY) =========================
        [Authorize(Roles = "Admin,Administrator")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllClients()
        {
            try
            {
                var clients = await _clientService.GetAllClientsAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all clients");
                return StatusCode(500, new { message = "Internal server error while fetching clients." });
            }
        }
        // ========================= Helpers =========================
        private int? GetUserIdFromJwt()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return null;
            return int.TryParse(claim.Value, out var id) ? id : null;
        }
    }
}

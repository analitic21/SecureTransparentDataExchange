using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Models.Shipping;
using SecureTransparentDataExchange.Services;
using System.Linq;
using  SecureTransparentDataExchange.DTOs;


namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        private readonly ShipmentService _shipmentService;
        private readonly ILogger<ShipmentController> _logger;

        public ShipmentController(
            ShipmentService shipmentService,
            ILogger<ShipmentController> logger)
        {
            _shipmentService = shipmentService;
            _logger = logger;
        }
        [Authorize(Roles = "Client,User,Admin,Administrator,Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentRequest request)
        {
            try
            {
                var loginId = GetLoginId();
                if (loginId == null)
                    return Unauthorized(new { message = "Invalid token." });

                if (request == null)
                    return BadRequest(new { message = "Request body is required." });

                var shipment = new Shipment
                {
                    PaymentStatus = request.PaymentStatus
                };

                var (success, message, createdShipment) =
                    await _shipmentService.CreateShipmentAsync(
                        shipment,
                        loginId.Value,
                        IsStaff(),
                        request.ClientId
                    );

                if (!success || createdShipment == null)
                    return BadRequest(new { message });

                return Ok(new
                {
                    message,
                    shipment = new
                    {
                        createdShipment.Id,
                        createdShipment.TrackingNumber,
                        createdShipment.Status,
                        createdShipment.PaymentStatus,
                        createdShipment.CreatedAt,
                        createdShipment.ClientId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shipment");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchByTracking([FromQuery] string trackingNumber)
        {
            try
            {
                var loginId = GetLoginId();
                if (loginId == null)
                    return Unauthorized();

                if (string.IsNullOrWhiteSpace(trackingNumber))
                    return BadRequest(new { message = "Tracking number is required." });

                var shipment = await _shipmentService.GetByTrackingNumberAsync(trackingNumber, loginId.Value);

                if (shipment == null)
                    return NotFound(new { message = "Shipment not found." });

                return Ok(new
                {
                    shipment.Id,
                    shipment.TrackingNumber,
                    shipment.Status,
                    shipment.PaymentStatus,
                    shipment.CreatedAt,
                    shipment.ClientId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching shipment by tracking number");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
        [Authorize(Roles = "Employee,Admin,Administrator")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllShipments()
        {
            try
            {
                var shipments = await _shipmentService.GetAllShipmentDtosAsync();
                return Ok(shipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all shipments");

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
        [Authorize(Roles = "Client,User,Admin,Administrator,Employee")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyShipments()
        {
            try
            {
                var loginId = GetLoginId();
                if (loginId == null)
                    return Unauthorized(new { message = "Invalid token." });

                var shipments = await _shipmentService.GetShipmentsByLoginIdAsync(loginId.Value);

                return Ok(shipments.Select(s => new
                {
                    s.Id,
                    s.TrackingNumber,
                    Status = s.Status.ToString(),
                    PaymentStatus = s.PaymentStatus.ToString(),
                    s.CreatedAt,
                    s.ClientId
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading my shipments");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var loginId = GetLoginId();
                if (loginId == null)
                    return Unauthorized();

                var ownsShipment = await _shipmentService.UserOwnsShipmentAsync(id, loginId.Value);

                if (!IsStaff() && !ownsShipment)
                    return Forbid();

                var shipment = await _shipmentService.GetByIdAsync(id);
                if (shipment == null)
                    return NotFound(new { message = "Shipment not found." });

                return Ok(new
                {
                    shipment.Id,
                    shipment.TrackingNumber,
                    shipment.Status,
                    shipment.PaymentStatus,
                    shipment.CreatedAt,
                    shipment.ClientId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shipment by id");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateShipment(int id, [FromBody] Shipment updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var loginId = GetLoginId();
                if (loginId == null)
                    return Unauthorized();

                var ownsShipment = await _shipmentService.UserOwnsShipmentAsync(id, loginId.Value);

                if (!IsStaff() && !ownsShipment)
                    return Forbid();

                var (success, message) =
                    await _shipmentService.UpdateDraftShipmentAsync(id, updated);

                if (!success)
                    return BadRequest(new { message });

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipment");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelShipment(int id)
        {
            try
            {
                var loginId = GetLoginId();
                if (loginId == null)
                    return Unauthorized();

                var ownsShipment = await _shipmentService.UserOwnsShipmentAsync(id, loginId.Value);

                if (!IsStaff() && !ownsShipment)
                    return Forbid();

                var (success, message) =
                    await _shipmentService.CancelShipmentAsync(id);

                if (!success)
                    return BadRequest(new { message });

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling shipment");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
        [Authorize(Roles = "Employee,Admin,Administrator")]
        [HttpPost("{id:int}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] AddShipmentItemRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || request.Quantity <= 0)
                return BadRequest(new { message = "Invalid item data." });

            try
            {
                var item = await _shipmentService.AddItemAsync(id, request.Name, request.Quantity);

                return Ok(new
                {
                    item.Id,
                    item.ShipmentId,
                    item.TrackingNumber,
                    item.Sender,
                    item.Receiver,
                    item.Status,
                    item.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to shipment {ShipmentId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }


        private bool IsStaff()
        {
            return User.IsInRole("Admin") ||
                   User.IsInRole("Administrator") ||
                   User.IsInRole("Employee");
        }

        private int? GetLoginId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id)
                ? id
                : null;
        }
    }
}
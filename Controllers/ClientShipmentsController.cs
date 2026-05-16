using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Models.Shipping;


namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/client/shipments")]
    [Authorize]
    public class ClientShipmentsController : ControllerBase
    {
        private readonly ShipmentService _shipmentService;
        private readonly ILogger<ClientShipmentsController> _logger;

        public ClientShipmentsController(
            ShipmentService shipmentService,
            ILogger<ClientShipmentsController> logger)
        {
            _shipmentService = shipmentService;
            _logger = logger;
        }

        // ===============================
        // GET api/client/shipments
        // ===============================
        [HttpGet]
        public async Task<IActionResult> GetMyShipments()
        {
            var loginId = GetLoginId();
            if (loginId == null)
                return Unauthorized();

            var shipments =
                await _shipmentService.GetShipmentsByLoginIdAsync(loginId.Value);

            return Ok(shipments);
        }

        // ===============================
        // GET api/client/shipments/{id}
        // ===============================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMyShipment(int id)
        {
            var loginId = GetLoginId();
            if (loginId == null)
                return Unauthorized();

            if (!await _shipmentService.UserOwnsShipmentAsync(id, loginId.Value))
                return Forbid();

            var shipment = await _shipmentService.GetByIdAsync(id);
            if (shipment == null)
                return NotFound();

            return Ok(shipment);
        }

        // ===============================
        // PUT api/client/shipments/{id}
        // ===============================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMyShipment(
            int id,
            [FromBody] Shipment shipment)
        {
            var loginId = GetLoginId();
            if (loginId == null)
                return Unauthorized();

            if (!await _shipmentService.UserOwnsShipmentAsync(id, loginId.Value))
                return Forbid();

            shipment.Id = id; // 🔐 защита от подмены ID

            var result =
                await _shipmentService.UpdateDraftShipmentAsync(id, shipment);

            if (!result.success)
                return BadRequest(new { result.message });

            return Ok(new { message = "Shipment updated successfully." });
        }

        // ===============================
        // POST api/client/shipments/{id}/cancel
        // ===============================
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelMyShipment(int id)
        {
            var loginId = GetLoginId();
            if (loginId == null)
                return Unauthorized();

            if (!await _shipmentService.UserOwnsShipmentAsync(id, loginId.Value))
                return Forbid();

            var result = await _shipmentService.CancelShipmentAsync(id);
            if (!result.success)
                return BadRequest(new { result.message });

            return Ok(new { message = "Shipment cancelled." });
        }

        // ===============================
        // HELPERS
        // ===============================
        private int? GetLoginId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id)
                ? id
                : null;
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using System.Security.Claims;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Client,Business,LegalEntity,Employee,Admin,Administrator")]
    public class DeliveryPriceController : ControllerBase
    {
        private readonly DeliveryPriceService _deliveryPriceService;
        private readonly ILogger<DeliveryPriceController> _logger;

        public DeliveryPriceController(
            DeliveryPriceService deliveryPriceService,
            ILogger<DeliveryPriceController> logger)
        {
            _deliveryPriceService = deliveryPriceService;
            _logger = logger;
        }

        // POST api/DeliveryPrice/calculate
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateDeliveryPrice(
            [FromBody] DeliveryPriceRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.OriginAddressId <= 0 || request.DestinationAddressId <= 0)
                return BadRequest(new { message = "Valid origin and destination address IDs are required." });

            if (request.OriginAddressId == request.DestinationAddressId)
                return BadRequest(new { message = "Origin and destination addresses must be different." });

            try
            {
                var loginId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    User.FindFirstValue("sub") ??
                    "unknown";

                _logger.LogInformation(
                    "Delivery price calculation requested by LoginId={LoginId}. Origin={OriginAddressId}, Destination={DestinationAddressId}",
                    loginId,
                    request.OriginAddressId,
                    request.DestinationAddressId
                );

                var response = await _deliveryPriceService.CalculateDeliveryPriceAsync(
                    request.OriginAddressId,
                    request.DestinationAddressId
                );

                if (!response.Success)
                    return BadRequest(new { message = response.Message });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error calculating delivery price. Origin={OriginAddressId}, Destination={DestinationAddressId}",
                    request.OriginAddressId,
                    request.DestinationAddressId
                );

                return StatusCode(500, new
                {
                    message = "Internal error while calculating delivery price."
                });
            }
        }
    }
}
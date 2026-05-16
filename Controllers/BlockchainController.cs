using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Administrator,Employee")]
    public class BlockchainController : ControllerBase
    {
        private readonly BlockchainValidationService _service;

        public BlockchainController(BlockchainValidationService service)
        {
            _service = service;
        }

        [HttpGet("validate/{shipmentId:int}")]
        public async Task<IActionResult> Validate(int shipmentId)
        {
            var isValid = await _service.ValidateShipmentChain(shipmentId);

            return Ok(new
            {
                shipmentId,
                isValid
            });
        }
    }
}
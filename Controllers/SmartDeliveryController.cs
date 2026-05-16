using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Services;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Administrator,Employee")]
    public class SmartDeliveryController : ControllerBase
    {
        private readonly SmartDeliveryService _service;

        public SmartDeliveryController(SmartDeliveryService service)
        {
            _service = service;
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] SmartDeliveryRequest request)
        {
            try
            {
                var result = await _service.ProcessAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
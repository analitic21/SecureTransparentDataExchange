using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services;

namespace SecureTransparentDataExchange.Controllers.Payments
{
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly StripeWebhookService _service;

        public StripeWebhookController(StripeWebhookService service)
        {
            _service = service;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Handle()
        {
            await _service.HandleAsync(Request);
            return Ok();
        }
    }
}
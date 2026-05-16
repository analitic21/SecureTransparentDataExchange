using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/system/encryption-keys")]
    [Authorize(Roles = "Admin")]
    public sealed class EncryptionKeyController : ControllerBase
    {
        private readonly EncryptedKeyService _service;

        public EncryptionKeyController(EncryptedKeyService service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns metadata of the active encryption key (NO key material).
        /// </summary>
        [HttpGet("active")]
        public IActionResult GetActiveKey()
        {
            var key = _service.GetActive();
            if (key == null)
            {
                return NotFound(new { message = "No active encryption key found." });
            }

            return Ok(new
            {
                key.Id,
                key.CreatedAt,
                key.ExpiresAt
            });
        }

        /// <summary>
        /// Rotates active encryption key.
        /// </summary>
        [HttpPost("rotate")]
        public IActionResult RotateKey()
        {
            _service.RotateActive();

            return Ok(new
            {
                message = "Encryption key rotated successfully."
            });
        }
    }
}

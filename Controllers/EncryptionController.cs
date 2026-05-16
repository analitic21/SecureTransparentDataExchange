using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services;

[Authorize(Roles = "Admin,Administrator")]
[ApiController]
[Route("api/encryption")]
public sealed class EncryptionController : ControllerBase
{
        private readonly EncryptionService _service;

        public EncryptionController(EncryptionService service)
        {
            _service = service;
        }

        [HttpPost("encrypt")]
        public async Task<IActionResult> Encrypt([FromBody] string plainText)
        {
            var encrypted = await _service.EncryptAsync(plainText);
            return Ok(encrypted);
        }

        [HttpPost("decrypt")]
        public async Task<IActionResult> Decrypt([FromBody] string encryptedText)
        {
            var decrypted = await _service.DecryptAsync(encryptedText);
            return Ok(decrypted);
        }
    }

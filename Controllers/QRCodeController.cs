using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Services;
using System.Security.Claims;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/qrcodes")]
    [Authorize]
    public class QRCodeController : ControllerBase
    {
        private readonly QRCodeStorageService _storage;
        private readonly AuditLogService _audit;

        public QRCodeController(
            QRCodeStorageService storage,
            AuditLogService audit)
        {
            _storage = storage;
            _audit = audit;
        }

        // ======================================
        // GET /api/qrcodes
        // ======================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _storage.GetAllAsync();
            return Ok(items);
        }

        // ======================================
        // GET /api/qrcodes/{id}
        // ======================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _storage.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        // ======================================
        // POST /api/qrcodes
        // ======================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QRCodeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _storage.CreateAsync(dto);

            await _audit.LogInfoAsync(
                "QR code created",
                User.Identity?.Name ?? "unknown",
                GetUserIdSafe()
            );

            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // ======================================
        // DELETE /api/qrcodes/{id}
        // ======================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _storage.DeleteAsync(id);
            if (!success)
                return NotFound();

            await _audit.LogWarningAsync(
                "QR code deleted",
                User.Identity?.Name ?? "unknown",
                GetUserIdSafe()
            );

            return NoContent();
        }

        // ======================================
        // Helper (SAFE)
        // ======================================
        private int? GetUserIdSafe()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}

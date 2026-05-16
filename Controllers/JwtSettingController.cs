using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/admin/jwt")]
    [Authorize(Roles = "Admin")]
    public class JwtSettingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JwtSettingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/admin/jwt/status
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var setting = await _context.JwtSettings
                .OrderByDescending(j => j.ExpiresAt)
                .Select(j => new
                {
                    j.Id,
                    j.Issuer,
                    j.Audience,
                    j.CreatedAt,
                    j.ExpiresAt,
                    EncryptionKeyId = j.EncryptionKeyId
                })
                .FirstOrDefaultAsync();

            if (setting == null)
                return NotFound(new { message = "JWT settings not initialized" });

            return Ok(setting);
        }
    }
}

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Services.Security;
using System.Security.Claims;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    public class TwoFactorController : ControllerBase
    {
        private readonly TwoFactorAuthService _twoFactorAuthService;
        private readonly LoginService _loginService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<TwoFactorController> _logger;

        public TwoFactorController(
            TwoFactorAuthService twoFactorAuthService,
            LoginService loginService,
            JwtTokenService jwtTokenService,
            ILogger<TwoFactorController> logger)
        {
            _twoFactorAuthService = twoFactorAuthService;
            _loginService = loginService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        // ======================================
        // POST /api/2fa/setup
        // QR code generation (does NOT include 2FA)
        // ======================================
        [Authorize]
        [HttpPost("setup")]
        public async Task<IActionResult> Setup()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _loginService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(user.TwoFactorSecretKey))
            {
                user.TwoFactorSecretKey = _twoFactorAuthService.GenerateSecretKey();
                user.IsTwoFactorEnabled = false;
                await _loginService.UpdateUserAsync(user);
            }

            var email = user.Email ?? $"{user.UserName}@2fa.local";

            var qrUri = _twoFactorAuthService.GenerateTwoFactorQrCode(
                email,
                user.TwoFactorSecretKey
            );

            var qrImage = _twoFactorAuthService.GenerateQrCodeAsPng(qrUri);

            return Ok(new
            {
                qrCodeBase64 = Convert.ToBase64String(qrImage),
                manualKey = user.TwoFactorSecretKey,
                qrUri
            });
        }

        // ======================================
        // POST /api/2fa/confirm
        // TOTP verification → INCLUDES 2FA
        // ======================================
        [Authorize]
        [EnableRateLimiting("twofa")]
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] TwoFactorVerifyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest("Code is required.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _loginService.GetUserByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecretKey))
                return BadRequest("2FA not initialized.");

            if (!_twoFactorAuthService.VerifyTotpCode(user.TwoFactorSecretKey, dto.Code))
                return Unauthorized("Invalid 2FA code.");

            user.IsTwoFactorEnabled = true;
            await _loginService.UpdateUserAsync(user);

            return Ok(new { message = "2FA enabled successfully." });
        }

        // ======================================
        // POST /api/2fa/verify
        // ЛОГИН С TOTP → ВЫДАЁТ JWT
        // ======================================
        [AllowAnonymous]
        [EnableRateLimiting("twofa")]
        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] TwoFactorLoginDto dto)
        {
            var user = await _loginService.GetUserByIdAsync(dto.UserId);

            if (user == null || !user.IsTwoFactorEnabled)
                return Unauthorized("2FA not enabled.");

            if (!_twoFactorAuthService.VerifyTotpCode(user.TwoFactorSecretKey!, dto.Code))
                return Unauthorized("Invalid 2FA code.");

            var jwt = await _jwtTokenService.GenerateJwtAsync(user);
            if (!jwt.Success)
                return StatusCode(500, jwt.Message);

            return Ok(new { token = jwt.Token });
        }

        // ======================================
        // POST /api/2fa/disable
        // ======================================
        [Authorize]
        [HttpPost("disable")]
        public async Task<IActionResult> Disable()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _loginService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound();

            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecretKey = null;
            await _loginService.UpdateUserAsync(user);

            return Ok(new { message = "Two-factor authentication disabled." });
        }
    }

    public class TwoFactorVerifyDto
    {
        public string Code { get; set; } = string.Empty;
    }

    public class TwoFactorLoginDto
    {
        public int UserId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}

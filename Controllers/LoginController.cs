using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SecureTransparentDataExchange.Models.API;
using SecureTransparentDataExchange.Models.API.Auth;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Services.Security;
using System.Security.Claims;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/login")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LoginController : ControllerBase
    {
        private readonly LoginService _loginService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AuditLogService _audit;

        public LoginController(
            LoginService loginService,
            JwtTokenService jwtTokenService,
            AuditLogService audit)
        {
            _loginService = loginService;
            _jwtTokenService = jwtTokenService;
            _audit = audit;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            var identifier =
                request.Email?.Trim().ToLower() ??
                request.Username?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(identifier))
                return BadRequest("Email or Username is required");

            var (isValid, user) =
                await _loginService.ValidateUserAsync(identifier, request.Password ?? "");

            if (!isValid || user == null)
            {
                await _audit.LogWarningAsync("Legacy login failed", identifier);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (user.IsTwoFactorEnabled)
            {
                await _audit.LogWarningAsync(
                    "Legacy login requires 2FA",
                    identifier,
                    user.Id
                );

                return Ok(new
                {
                    requiresTwoFactor = true,
                    requires2FA = true,
                    userId = user.Id,
                    message = "Two-factor authentication required."
                });
            }

            var jwt = await _jwtTokenService.GenerateJwtAsync(user);
            if (!jwt.Success)
                return StatusCode(500, new { message = jwt.Message });

            await _audit.LogInfoAsync("Legacy login success", identifier, user.Id);

            return Ok(new
            {
                token = jwt.Token,
                user.Id,
                user.Email,
                user.UserName,
                role = user.Role?.Name ?? "User",
                user.UserType
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out var userId))
                return Unauthorized();

            var user = await _loginService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Email,
                user.UserName,
                role = user.Role?.Name ?? "User",
                user.UserType,
                user.IsTwoFactorEnabled
            });
        }
    }
}
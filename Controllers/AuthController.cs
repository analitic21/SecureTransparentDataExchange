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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginService _loginService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AuditLogService _audit;

        public AuthController(
            LoginService loginService,
            JwtTokenService jwtTokenService,
            AuditLogService audit)
        {
            _loginService = loginService;
            _jwtTokenService = jwtTokenService;
            _audit = audit;
        }

        // ===========================================
        // LOGIN
        // ===========================================
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticateRequest request)
        {
            var identifier =
                request.Email?.Trim().ToLower() ??
                request.Username?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(identifier))
                return BadRequest(new { message = "Email or Username is required" });

            var (isValid, user) =
                await _loginService.ValidateUserAsync(identifier, request.Password ?? "");

            if (!isValid || user == null)
            {
                await _audit.LogWarningAsync("Login failed", identifier);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // 🔐 2FA REQUIRED
            if (user.IsTwoFactorEnabled)
            {
                return Ok(new
                {
                    requiresTwoFactor = true,
                    userId = user.Id
                });
            }

            var jwt = await _jwtTokenService.GenerateJwtAsync(user);
            if (!jwt.Success)
                return StatusCode(500, new { message = jwt.Message });

            await _audit.LogInfoAsync("Login success", identifier, user.Id);

            return Ok(new
            {
                requiresTwoFactor = false,
                token = jwt.Token,
                userId = user.Id,
                email = user.Email,
                username = user.UserName,
                userType = user.UserType
            });
        }

        // ===========================================
        // REGISTER
        // ===========================================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _loginService.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // ===========================================
        // CURRENT USER
        // ===========================================
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
                userId = user.Id,
                email = user.Email,
                username = user.UserName,
                role = user.Role?.Name ?? "User",
                userType = user.UserType,
                isTwoFactorEnabled = user.IsTwoFactorEnabled
            });
        }
    }
}
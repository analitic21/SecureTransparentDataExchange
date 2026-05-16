using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Services.Security;
using SecureTransparentDataExchange.Models.API.Auth;


namespace SecureTransparentDataExchange.Controllers.API
{
    [ApiController]
    [Route("api/token")]
    public class RefreshTokenController : ControllerBase
    {
        private readonly RefreshTokenService _refreshTokenService;
        private readonly ILogger<RefreshTokenController> _logger;

        public RefreshTokenController(
            RefreshTokenService refreshTokenService,
            ILogger<RefreshTokenController> logger)
        {
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Refresh JWT using refresh token
        /// </summary>

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { message = "Refresh token is required." });

            var result = await _refreshTokenService.RefreshAsync(request.RefreshToken);

            if (!result.Success)
            {
                _logger.LogWarning("Refresh token failed: {Message}", result.Message);
                return Unauthorized(new { message = result.Message });
            }

            return Ok(new
            {
                token = result.Token
            });
        }
    }
}

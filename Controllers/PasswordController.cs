using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Models.API;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Services.Security;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/password")]
    public class PasswordController : ControllerBase
    {
        private readonly PasswordService _passwordService;
        private readonly ILogger<PasswordController> _logger;

        public PasswordController(
            PasswordService passwordService,
            ILogger<PasswordController> logger)
        {
            _passwordService = passwordService;
            _logger = logger;
        }

        // ============================================
        // POST: api/password/forgot
        // ============================================
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var code = await _passwordService
                .RequestPasswordResetAsync(model.Identifier);

            if (code == null)
                return NotFound(new { message = "User not found" });

            _logger.LogInformation(
                "Password recovery initiated for {Identifier}",
                model.Identifier
            );

            // ⛔ код НЕ возвращаем клиенту (email / sms)
            return Ok(new { message = "Recovery code sent" });
        }

        // ============================================
        // POST: api/password/reset
        // ============================================
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _passwordService
                .RecoverPasswordAsync(model.Code, model.NewPassword);

            if (!success)
                return BadRequest(new
                {
                    message = "Invalid or expired recovery code"
                });

            return Ok(new
            {
                message = "Password reset successfully"
            });
        }

        // ============================================
        // POST: api/password/change (JWT ONLY)
        // ============================================
        [Authorize]
        [HttpPost("change")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();

            var success = await _passwordService
                .ChangePasswordAsync(
                    userId,
                    model.CurrentPassword,
                    model.NewPassword
                );

            if (!success)
                return BadRequest(new
                {
                    message = "Invalid current password"
                });

            return Ok(new
            {
                message = "Password changed successfully"
            });
        }
    }
}

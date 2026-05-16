using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailModel emailModel)
        {
            if (emailModel == null)
                return BadRequest("Email data is required.");

            if (string.IsNullOrWhiteSpace(emailModel.To) ||
                string.IsNullOrWhiteSpace(emailModel.Subject))
                return BadRequest("Recipient and subject are required.");

            try
            {
                byte[]? qrBytes = null;

                if (!string.IsNullOrWhiteSpace(emailModel.QrCodeBase64))
                    qrBytes = Convert.FromBase64String(emailModel.QrCodeBase64);

                await _emailService.SendEmailAsync(
                    emailModel.To,
                    emailModel.Subject,
                    emailModel.Body ?? string.Empty,
                    qrBytes
                );

                return Ok(new { message = "Email sent successfully." });
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Invalid QR code Base64 format." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending email", error = ex.Message });
            }
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Services;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/sms")]
    [Authorize]
    public class SmsController : ControllerBase
    {
        private readonly SmsService _smsService;

        public SmsController(SmsService smsService)
        {
            _smsService = smsService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendSms([FromBody] SendSmsRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest("Phone number is required.");

            if (string.IsNullOrWhiteSpace(request.TrackingNumber))
                return BadRequest("Tracking number is required.");

            var sms = new SmsModel(
                request.PhoneNumber,
                request.ShipmentStatus,
                request.TrackingNumber
            );

            await _smsService.SendSmsAsync(sms);

            return Ok(new
            {
                message = "SMS sent successfully.",
                request.PhoneNumber,
                request.TrackingNumber,
                status = request.ShipmentStatus.ToString()
            });
        }
    }

    public class SendSmsRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public ShipmentStatus ShipmentStatus { get; set; }
    }
}
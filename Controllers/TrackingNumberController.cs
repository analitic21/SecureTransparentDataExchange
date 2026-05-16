using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Repositories;
using SecureTransparentDataExchange.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackingNumberController : ControllerBase
    {
        private readonly TrackingNumberService _trackingNumberService;
        private readonly TrackingNumberRepository _trackingNumberRepository;
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;
        private readonly ILogger<TrackingNumberController> _logger;

        public TrackingNumberController(
            TrackingNumberService trackingNumberService,
            TrackingNumberRepository trackingNumberRepository,
            EmailService emailService,
            SmsService smsService,

            ILogger<TrackingNumberController> logger)
        {
            _trackingNumberService = trackingNumberService;
            _trackingNumberRepository = trackingNumberRepository;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        // =====================================================
        // Generate + Send tracking number
        // =====================================================
        [HttpPost("generate-and-send")]
        public async Task<IActionResult> GenerateAndSendTrackingNumber(
            [FromBody] TrackingNumberRequest request)
        {
            try
            {
                var tracking = _trackingNumberService.GenerateTrackingNumber();

                _logger.LogInformation(
                    "Generated TrackingNumber: {TrackingNumber}",
                    tracking.TrackingNumber);

                var entity = new TrackingNumberEntity
                {
                    TrackingNumber = tracking.TrackingNumber,
                    ShipmentStatus = ShipmentStatus.Created
                };

                await _trackingNumberRepository.AddAsync(entity);

                // ---------- EMAIL ----------
                var emailTask = _emailService.SendEmailAsync(
                    request.Email,
                    "Your Tracking Number",
                    $"Your tracking number is {entity.TrackingNumber}. Status: {entity.ShipmentStatus}",
                    null
                );

                // ---------- SMS (КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ) ----------
                var smsModel = new SmsModel(
                  request.PhoneNumber,
                 entity.ShipmentStatus,
                 entity.TrackingNumber
);

                var smsTask = _smsService.SendSmsAsync(smsModel);


                

                await Task.WhenAll(emailTask, smsTask);

                return Ok(new
                {
                    message = "Email and SMS sent successfully.",
                    trackingNumber = entity.TrackingNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tracking number generation failed");
                return StatusCode(500, new
                {
                    message = "Error generating tracking number",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // Update tracking status
        // =====================================================
        [HttpPut("update-status/{id:int}")]
        public async Task<IActionResult> UpdateTrackingStatus(
            int id,
            [FromBody] ShipmentStatus newStatus)
        {
            var tracking = await _trackingNumberRepository
                .GetTrackingNumberByIdAsync(id);

            if (tracking == null)
            {
                return NotFound(new { message = "Tracking number not found." });
            }

            tracking.UpdateShipmentStatus(newStatus);
            await _trackingNumberRepository.UpdateTrackingNumberAsync(tracking);

            return Ok(new
            {
                message = "Tracking status updated successfully.",
                trackingNumber = tracking.TrackingNumber,
                status = newStatus
            });
        }
    }
}

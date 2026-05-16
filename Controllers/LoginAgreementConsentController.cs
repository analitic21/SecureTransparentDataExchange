using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/login-agreement-consents")]
    [ApiController]
    public class LoginAgreementConsentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginAgreementConsentController> _logger;

        public LoginAgreementConsentController(ApplicationDbContext context, ILogger<LoginAgreementConsentController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all login agreement consents.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllConsents()
        {
            var consents = await _context.LoginAgreementConsents
                .Include(c => c.Login)
                .Include(c => c.Agreement)
                .ToListAsync();

            return Ok(consents);
        }

        /// <summary>
        /// Get login agreements accepted by a specific user.
        /// </summary>
        [HttpGet("user/{loginId}")]
        public async Task<IActionResult> GetConsentsByUserId(int loginId)
        {
            var consents = await _context.LoginAgreementConsents
                .Include(c => c.Agreement)
                .Where(c => c.LoginId == loginId)
                .ToListAsync();

            return Ok(consents);
        }

        /// <summary>
        /// Check if a user has accepted a specific login agreement.
        /// </summary>
        [HttpGet("user/{loginId}/agreement/{agreementId}")]
        public async Task<IActionResult> HasUserAcceptedAgreement(int loginId, int agreementId)
        {
            bool accepted = await _context.LoginAgreementConsents
                .AnyAsync(c => c.LoginId == loginId && c.AgreementId == agreementId);

            return Ok(new { accepted });
        }

        /// <summary>
        /// Accept a login agreement.
        /// </summary>
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptAgreement([FromBody] LoginAgreementConsent consent)
        {
            if (consent == null)
                return BadRequest(new { message = "Invalid data" });

            // Check if the user exists
            var login = await _context.Users.FindAsync(consent.LoginId);
            var agreement = await _context.LoginAgreements.FindAsync(consent.AgreementId);

            if (login == null)
                return NotFound(new { message = "User not found" });

            if (agreement == null)
                return NotFound(new { message = "Agreement not found" });

            // Check if the user has already accepted this agreement
            var existingConsent = await _context.LoginAgreementConsents
                .FirstOrDefaultAsync(c => c.LoginId == consent.LoginId && c.AgreementId == consent.AgreementId);

            if (existingConsent != null)
                return BadRequest(new { message = "Agreement already accepted by this user." });

            // Set the acceptance date
            consent.AcceptedAt = DateTime.UtcNow;

            // Add the consent to the database
            _context.LoginAgreementConsents.Add(consent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Agreement accepted", data = consent });
        }
    }
}

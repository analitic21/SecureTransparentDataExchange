using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SecureTransparentDataExchange.Services
{
    public class LoginAgreementConsentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginAgreementConsentService> _logger;

        public LoginAgreementConsentService(ApplicationDbContext context, ILogger<LoginAgreementConsentService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get a list of all user consents.
        /// </summary>
        public async Task<List<LoginAgreementConsent>> GetAllConsentsAsync()
        {
            return await _context.LoginAgreementConsents
            .Include(c => c.Login)
            .Include(c => c.Agreement)
            .ToListAsync();
        }

        /// <summary>
        /// Add a new user consent.
        /// Check for unique consent for user and agreement.
        /// </summary>
        public async Task<bool> AddConsentAsync(LoginAgreementConsent consent)
        {
            try
            {
                // Check for existing consent
                if (await IsConsentExisting(consent.LoginId, consent.AgreementId))
                {
                    _logger.LogWarning("User has already accepted the agreement.");
                    return false; // Return false if consent already exists
                }

                // Automatically set default values
                if (consent.AcceptedAt == default)
                {
                    consent.AcceptedAt = DateTime.UtcNow; // Set the current date as the acceptance date
                }

                if (!consent.ConsentGiven)
                {
                    consent.ConsentGiven = true; // Set the consent status to "given"
                }

                // Add a new consent to the database
                _context.LoginAgreementConsents.Add(consent);
                await _context.SaveChangesAsync();

                // Articles are not added automatically, you need to add them manually via the API
                _logger.LogInformation("User consent added successfully.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user consent.");
                return false;
            }
        }

        /// <summary>
        /// Check for the existence of consent for the user and the agreement.
        /// </summary> 
        private async Task<bool> IsConsentExisting(int loginId, int agreementId)
        {
            return await _context.LoginAgreementConsents
            .AnyAsync(c => c.LoginId == loginId && c.AgreementId == agreementId);
        }

        /// <summary> 
        /// Remove user consent by ID. 
        /// </summary> 
        public async Task<bool> DeleteConsentAsync(int id)
        {
            var consent = await _context.LoginAgreementConsents.FindAsync(id);
            if (consent == null)
                return false;

            _context.LoginAgreementConsents.Remove(consent);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary> 
        /// Update user consent. 
        /// </summary> 
        public async Task<bool> UpdateConsentAsync(int id, LoginAgreementConsent updatedConsent)
        {
            var consent = await _context.LoginAgreementConsents.FindAsync(id);
            if (consent == null)
                return false;

            consent.ConsentGiven = updatedConsent.ConsentGiven;
            consent.AcceptedAt = updatedConsent.AcceptedAt;

            _context.LoginAgreementConsents.Update(consent);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services
{
    public class LoginAgreementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginAgreementService> _logger;

        public LoginAgreementService(ApplicationDbContext context, ILogger<LoginAgreementService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Add a new agreement
        /// </summary>
        public async Task<bool> AddAgreementAsync(LoginAgreement agreement)
        {
            try
            {
                agreement.CreatedAt = DateTime.UtcNow;

                _context.LoginAgreements.Add(agreement);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Agreement {AgreementId} added.", agreement.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agreement.");
                return false;
            }
        }

        /// <summary>
        /// Get an agreement by ID
        /// </summary>
        public async Task<LoginAgreement?> GetAgreementByIdAsync(int id)
        {
            return await _context.LoginAgreements
                .Include(a => a.Articles)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Get ALL agreements
        /// </summary>
        public async Task<List<LoginAgreement>> GetAllAgreementsAsync()
        {
            return await _context.LoginAgreements
                .Include(a => a.Articles)
                .OrderByDescending(a => a.Version)
                .ToListAsync();
        }

        /// <summary>
        /// Get the latest agreement (used by frontend)
        /// </summary>
        public async Task<LoginAgreement?> GetLatestAgreementAsync()
        {
            var latest = await _context.LoginAgreements
                .OrderByDescending(a => a.Version)
                .Include(a => a.Articles)
                .FirstOrDefaultAsync();

            if (latest == null)
            {
                _logger.LogWarning("No agreements found.");
            }

            return latest;
        }

        /// <summary>
        /// Update agreement
        /// </summary>
        public async Task<bool> UpdateAgreementAsync(int id, LoginAgreement updated)
        {
            var agreement = await _context.LoginAgreements.FindAsync(id);
            if (agreement == null)
                return false;

            agreement.Version = updated.Version;
            agreement.Content = updated.Content;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Delete agreement
        /// </summary>
        public async Task<bool> DeleteAgreementAsync(int id)
        {
            var agreement = await _context.LoginAgreements.FindAsync(id);
            if (agreement == null)
                return false;

            _context.LoginAgreements.Remove(agreement);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

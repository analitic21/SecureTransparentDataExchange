using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Services
{
    public class PasswordService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PasswordService> _logger;
        private readonly PasswordHasher<Login> _passwordHasher;

        public PasswordService(
            ApplicationDbContext context,
            ILogger<PasswordService> logger,
            PasswordHasher<Login> passwordHasher)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        // ====================================================
        // REQUEST PASSWORD RESET (send recovery code)
        // ====================================================
        public async Task<string?> RequestPasswordResetAsync(string identifier)
        {
            identifier = identifier.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email != null && u.Email.ToLower() == identifier ||
                u.PhoneNumber != null && u.PhoneNumber == identifier
            );

            if (user == null)
                return null;

            var recoveryCode = Guid.NewGuid().ToString("N");

            user.RecoveryCode = recoveryCode;
            user.RecoveryCodeExpiry = DateTime.UtcNow.AddMinutes(15);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Password recovery code generated for userId {UserId}",
                user.Id
            );

            return recoveryCode;
        }

        // ====================================================
        // CONFIRM RECOVERY CODE & SET NEW PASSWORD
        // ====================================================
        public async Task<bool> RecoverPasswordAsync(string recoveryCode, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.RecoveryCode == recoveryCode &&
                u.RecoveryCodeExpiry != null &&
                u.RecoveryCodeExpiry > DateTime.UtcNow
            );

            if (user == null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.RecoveryCode = null;
            user.RecoveryCodeExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Password successfully reset for userId {UserId}",
                user.Id
            );

            return true;
        }

        // ====================================================
        // CHANGE PASSWORD (authorized user)
        // ====================================================
        public async Task<bool> ChangePasswordAsync(
            int userId,
            string currentPassword,
            string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            var verification = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                currentPassword
            );

            if (verification != PasswordVerificationResult.Success)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Password changed successfully for userId {UserId}",
                user.Id
            );

            return true;
        }
    }
}

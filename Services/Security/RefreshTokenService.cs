using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.API.Auth;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services.Security
{
    public class RefreshTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(
            ApplicationDbContext context,
            JwtTokenService jwtTokenService,
            ILogger<RefreshTokenService> logger)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        // =========================
        // REFRESH TOKEN
        // =========================
        public async Task<RefreshTokenResult> RefreshAsync(string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == refreshToken &&
                    t.ExpiryDate > DateTime.UtcNow);

            // ❌ invalid token
            if (tokenEntity == null)
            {
                return new RefreshTokenResult(false, "Invalid or expired refresh token", null);
            }

            // ❌ user missing
            if (tokenEntity.User == null)
            {
                return new RefreshTokenResult(false, "User not found", null);
            }

            // ⛔ user locked
            if (tokenEntity.User.LockoutEnd.HasValue &&
                tokenEntity.User.LockoutEnd > DateTimeOffset.UtcNow)
            {
                return new RefreshTokenResult(false, "User is locked", null);
            }

            // 🔐 generate new JWT
            var jwtResult = await _jwtTokenService.GenerateJwtAsync(tokenEntity.User);

            if (!jwtResult.Success)
            {
                return new RefreshTokenResult(false, jwtResult.Message, null);
            }

            // 🔥 invalidate refresh token (no migration)
            tokenEntity.ExpiryDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new RefreshTokenResult(
                true,
                "Token refreshed",
                jwtResult.Token
            );
        }


        // =========================
        // GENERATE TOKEN
        // =========================
        private static string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}

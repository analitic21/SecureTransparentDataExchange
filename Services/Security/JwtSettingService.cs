using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Security;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services.Security
{
    public class JwtSettingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtSettingService> _logger;
        private readonly EncryptionService _encryptionService;

        public JwtSettingService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<JwtSettingService> logger,
            EncryptionService encryptionService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        public async Task<JwtRuntimeConfig> GetOrCreateJwtConfigAsync()
        {
            var now = DateTime.UtcNow;

            // 1️⃣ Try existing valid JWT
            var jwt = await _context.JwtSettings
                .Where(j => j.ExpiresAt > now)
                .OrderByDescending(j => j.ExpiresAt)
                .FirstOrDefaultAsync();

            if (jwt != null)
            {
                return new JwtRuntimeConfig
                {
                    SecretKey = await _encryptionService.DecryptAsync(jwt.EncryptedSecretKey),
                    Issuer = jwt.Issuer,
                    Audience = jwt.Audience,
                    AccessTokenLifetimeMinutes = 120
                };
            }

            // 2️⃣ GET OR CREATE EncryptionKey
            var encryptionKey = await _context.EncryptionKeys.FirstOrDefaultAsync();

            if (encryptionKey == null)
            {
                _logger.LogWarning("EncryptionKey not found. Creating initial encryption key.");

                encryptionKey = new EncryptionKey();
                encryptionKey.RotateKeys();

                _context.EncryptionKeys.Add(encryptionKey);
                await _context.SaveChangesAsync();
            }

            // 3️⃣ Create new JWT
            var rawKey = GenerateKey();
            var encrypted = await _encryptionService.EncryptAsync(rawKey);

            jwt = new JwtSetting
            {
                EncryptedSecretKey = encrypted,
                Issuer = _configuration["Jwt:Issuer"] ?? "STDExchange",
                Audience = _configuration["Jwt:Audience"] ?? "STDExchange.Client",
                CreatedAt = now,
                ExpiresAt = now.AddYears(1),
                EncryptionKeyId = encryptionKey.Id
            };

            _context.JwtSettings.Add(jwt);
            await _context.SaveChangesAsync();

            return new JwtRuntimeConfig
            {
                SecretKey = rawKey,
                Issuer = jwt.Issuer,
                Audience = jwt.Audience,
                AccessTokenLifetimeMinutes = 120
            };
        }

        private static string GenerateKey()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}

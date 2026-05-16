using System;
using System.Net;
using Microsoft.Extensions.Logging;
using OtpNet;
using QRCoder;

namespace SecureTransparentDataExchange.Services
{
    public class TwoFactorAuthService
    {
        private readonly ILogger<TwoFactorAuthService> _logger;

        public TwoFactorAuthService(ILogger<TwoFactorAuthService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        public string GenerateTwoFactorQrCode(string email, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("SecretKey is required", nameof(secretKey));

            email = string.IsNullOrWhiteSpace(email)
                ? "user@securetransparentdataexchange"
                : email;

            var encodedSecret = WebUtility.UrlEncode(secretKey);
            var encodedEmail = WebUtility.UrlEncode(email);

            return
                $"otpauth://totp/SecureTransparentDataExchange:{encodedEmail}" +
                $"?secret={encodedSecret}&issuer=SecureTransparentDataExchange";
        }

        public byte[] GenerateQrCodeAsPng(string content)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(data);

            return qr.GetGraphic(20);
        }

        public bool VerifyTotpCode(string secretKey, string userCode)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                return false;

            if (string.IsNullOrWhiteSpace(userCode))
                return false;

            userCode = userCode.Trim().Replace(" ", "");

            try
            {
                var secretBytes = Base32Encoding.ToBytes(secretKey.Trim());
                var totp = new Totp(secretBytes);

                return totp.VerifyTotp(
                    userCode,
                    out _,
                    new VerificationWindow(previous: 2, future: 2)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "2FA verification failed");
                return false;
            }
        }
    }
}
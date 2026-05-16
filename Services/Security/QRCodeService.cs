using System;
using QRCoder;

namespace SecureTransparentDataExchange.Services.Security
{
    public class QRCodeService
    {
        // Generation of Base64 QR-code 
        public string GenerateQrCodeBase64(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("QR content is empty");

            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(data);

            var bytes = qr.GetGraphic(20);
            return Convert.ToBase64String(bytes);
        }

        // otpauth URI for 2FA 
        public string GenerateOtpAuthUri(string email, string secret)
        {
            return $"otpauth://totp/SecureLogistics:{email}?secret={secret}&issuer=SecureLogistics";
        }
    }
}
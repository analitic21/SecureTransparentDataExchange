using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using System;
using System.Security.Cryptography;

namespace SecureTransparentDataExchange.Services
{
    public class TrackingNumberService
    {
        // =========================
        // Generate tracking number
        // =========================
        public TrackingNumberEntity GenerateTrackingNumber()
        {
            var trackingNumber = GenerateSecureTrackingNumber();

            return new TrackingNumberEntity
            {
                TrackingNumber = trackingNumber,
                ShipmentStatus = ShipmentStatus.Created,
                CreatedAt = DateTime.UtcNow
            };
        }

        // =========================
        // Update status (domain logic)
        // =========================
        public void UpdateTrackingStatus(
            TrackingNumberEntity trackingNumber,
            ShipmentStatus newStatus)
        {
            ArgumentNullException.ThrowIfNull(trackingNumber);
            trackingNumber.ShipmentStatus = newStatus;
        }

        // =========================
        // Secure generator
        // =========================
        private static string GenerateSecureTrackingNumber()
        {
            // 9 digits, cryptographically safe
            Span<byte> buffer = stackalloc byte[4];
            RandomNumberGenerator.Fill(buffer);

            var number = BitConverter.ToUInt32(buffer) % 1_000_000_000;
            return number.ToString("D9");
        }
    }
}

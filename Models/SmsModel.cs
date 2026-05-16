using System;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Domain model for SMS notification.
    /// </summary>
    public class SmsModel
    {
        public SmsModel(string phoneNumber, ShipmentStatus status, string trackingNumber)
        {
            PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
            Status = status;
            TrackingNumber = trackingNumber ?? throw new ArgumentNullException(nameof(trackingNumber));
        }

        [Required]
        [Phone]
        public string PhoneNumber { get; }

        [Required]
        public ShipmentStatus Status { get; }

        [Required]
        [StringLength(50)]
        public string TrackingNumber { get; }

        /// <summary>
        /// Returns human-readable message for SMS.
        /// </summary>
        public string BuildMessage()
        {
            return Status switch
            {
                ShipmentStatus.Created =>
                    $"📦 Shipment {TrackingNumber} has been created.",

                ShipmentStatus.PaymentCompleted =>
                    $"✅ Payment received for shipment {TrackingNumber}. Courier will be assigned soon.",

                ShipmentStatus.InTransit =>
                    $"🚚 Shipment {TrackingNumber} is on the way.",

                ShipmentStatus.Delivered =>
                    $"📬 Shipment {TrackingNumber} has been delivered.",

                ShipmentStatus.Canceled =>
                    $"❌ Shipment {TrackingNumber} was canceled.",

                _ =>
                    $"ℹ️ Shipment {TrackingNumber} status updated: {Status}"
            };
        }
    }
}

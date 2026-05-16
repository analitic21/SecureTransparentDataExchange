using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Shipping;


namespace SecureTransparentDataExchange.Models
{
    public class Package
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TrackingNumber { get; set; } = string.Empty;

        public string Sender { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // CargoType
        public int? CargoTypeId { get; set; }
        public virtual CargoType? CargoType { get; set; }

        // IoTDevice
        public virtual IoTDevice? IoTDevice { get; set; }

        // Shipment (not through ShipmentClient)
        public int? ShipmentId { get; set; }
        public virtual Shipment? Shipment { get; set; }

        // Created / Updated by Login (user)
        public int? CreatedByUserId { get; set; }
        public virtual Login? CreatedByUser { get; set; }

        public int? UpdatedByUserId { get; set; }
        public virtual Login? UpdatedByUser { get; set; }
    }
}

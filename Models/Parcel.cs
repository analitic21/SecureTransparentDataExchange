using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Shipping;

namespace SecureTransparentDataExchange.Models
{
    public class Parcel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TrackingNumber { get; set; } = string.Empty;

        public string Sender { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightKg { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CargoValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PackageCost { get; set; }

        [NotMapped]
        public decimal TotalCost => CargoValue + PackageCost;

        public int? CargoTypeId { get; set; }

        [ForeignKey(nameof(CargoTypeId))]
        public virtual CargoType? CargoType { get; set; }

        public int? ShipmentId { get; set; }

        [ForeignKey(nameof(ShipmentId))]
        public virtual Shipment? Shipment { get; set; }

        public virtual IoTDevice? IoTDevice { get; set; }

        public int? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual Login? CreatedByUser { get; set; }

        public int? UpdatedByUserId { get; set; }

        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual Login? UpdatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
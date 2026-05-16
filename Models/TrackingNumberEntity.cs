using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Shipping;

namespace SecureTransparentDataExchange.Models
{
    public class TrackingNumberEntity
    {
        [Key]
        public int Id { get; set; }

        // Use a string with a default value of "0" for the tracking number
        [Required]
        public string TrackingNumber { get; set; } = "0"; // Set "0" as the default string

        // Default value for the status
        [Required]
        public ShipmentStatus ShipmentStatus { get; set; } = ShipmentStatus.Created; // Default status

        // Date and time of record creation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Set current time as default value

        // Relationship to Shipment model (one-to-many)
        [Required]
        public int ShipmentId { get; set; } // Foreign key for relationship with Shipment

        [ForeignKey("ShipmentId")]
        public Shipment Shipment { get; set; } = new Shipment(); // Initialize Shipment by default

        // Add UserId
        [Required]
        public string UserId { get; set; } = string.Empty; // Set default string

        // Constructor to initialize Shipment and ShipmentStatus
        public TrackingNumberEntity()
        {
            // Here values ​​are already set by default, so no need to initialize them again
        }

        // Method to change status dynamically
        public void UpdateShipmentStatus(ShipmentStatus newStatus)
        {
            ShipmentStatus = newStatus; // Update tracking status
        }
    }
}
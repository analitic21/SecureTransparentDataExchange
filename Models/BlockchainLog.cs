using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Shipping;


namespace SecureTransparentDataExchange.Models
{
    public class BlockchainLog
    {
        [Key]
        public int Id { get; set; } // Primary key

        // Shipment association
        [Required]
        public int ShipmentId { get; set; } // Shipment identifier

        [ForeignKey(nameof(ShipmentId))]
        public virtual Shipment Shipment { get; set; } = null!; // Navigation property

        [Required]
        [StringLength(255)]
        public string BlockHash { get; set; } = string.Empty; // Current block hash

        [Required]
        [StringLength(255)]
        public string PreviousBlockHash { get; set; } = string.Empty; // Previous block hash

        [Required]
        public string Data { get; set; } = string.Empty; // Block data

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Block creation time

        // User association (Login)
        [Required]
        public int LoginId { get; set; } // Who created the block

        [ForeignKey(nameof(LoginId))]
        public virtual Login Login { get; set; } = null!; // Navigation property

        // Association with system logs (SystemLog)
        public virtual ICollection<SystemLog> SystemLogs { get; set; } = new HashSet<SystemLog>(); // Initialize the collection

        [StringLength(500)]
        public string? Description { get; set; } // Block description (optional)

        // Constructor
        public BlockchainLog()
        {
            SystemLogs = new HashSet<SystemLog>();
        }
    }
}

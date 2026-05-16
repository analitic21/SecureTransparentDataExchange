using System;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.Shipping;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Class for storing system logs.
    /// </summary>
    public class SystemLog
    {
        /// <summary>
        /// Unique identifier of the system log.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Event type (e.g. error, action, warning).
        /// </summary>
        [Required(ErrorMessage = "Event type is required.")]
        [MaxLength(50, ErrorMessage = "Event type cannot exceed 50 characters.")]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Log message.
        /// </summary>
        [Required(ErrorMessage = "Message is required.")]
        [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Log timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User association (if applicable).
        /// </summary>
        public int? UserId { get; set; }
        public Login? User { get; set; }

        /// <summary>
        /// Shipment association (if applicable).
        /// </summary>
        public int? ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        /// <summary>
        /// Link to the blockchain entry (if applicable).
        /// </summary>
        public int? BlockchainLogId { get; set; }
        public BlockchainLog? BlockchainLog { get; set; }
    }
}
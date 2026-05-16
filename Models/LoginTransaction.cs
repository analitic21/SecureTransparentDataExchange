using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Shipping;


namespace SecureTransparentDataExchange.Models
{
    public class LoginTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Login User { get; set; } = null!;

        [Required]
        public ActionType ActionType { get; set; } = ActionType.Created;

        [Required]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Details { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Payment amount must be a positive value.")]
        public decimal? PaymentAmount { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // Fix: Correct assignment of ShipmentStatus value 
        public ShipmentStatus ShipmentStatus { get; set; } = ShipmentStatus.Pending;

        public int? ShipmentId { get; set; }

        [ForeignKey("ShipmentId")]
        public Shipment? Shipment { get; set; }


        public int? ProcessedByEmployeeId { get; set; }

        [ForeignKey("ProcessedByEmployeeId")]
        public Employee? ProcessedByEmployee { get; set; }
    }
}
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.Shipping;
using SecureTransparentDataExchange.Models.Billing;

namespace SecureTransparentDataExchange.Models.Orders
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // =========================
        // ORDER NUMBER
        // =========================
        [Required, MaxLength(32)]
        public string OrderNumber { get; set; } = string.Empty;

        // =========================
        // WHO CREATED
        // =========================
        [Required]
        public int LoginId { get; set; }

        [ForeignKey(nameof(LoginId))]
        public Login? Login { get; set; }

        // USER TYPE
        // =========================
        [Required]
        public UserType UserType { get; set; }

        // =========================
        // INDIVIDUAL
        // =========================
        public int? ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public Client? Client { get; set; }

        // =========================
        // LEGAL ENTITY
        // =========================
        
        public int? LegalEntityId { get; set; }
        public LegalEntity? LegalEntity { get; set; }
       
      
        // =========================
        // SHIPMENT (OPTIONAL)
        // =========================
        public int? ShipmentId { get; set; }

        [ForeignKey(nameof(ShipmentId))]
        public Shipment? Shipment { get; set; }

        // =========================
        // PAYMENT SUMMARY
        // =========================
        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public Invoice? Invoice { get; set; }
        [Required, MaxLength(3)]
        public string Currency { get; set; } = "EUR";
        // =========================
        // PAYMENT
        // =========================;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        
        // =========================
        // EXTERNAL
        // =========================
        [MaxLength(100)]
        public string? ExternalOrderNumber { get; set; }

        [Required, MaxLength(50)]
        public string Source { get; set; } = "Web";

        // =========================
        // AUDIT
        // =========================
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

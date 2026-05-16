using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Shipping;
using SecureTransparentDataExchange.Models.Billing;

namespace SecureTransparentDataExchange.Models.Billing
{
    // Unique only when TransactionId exists
    [Index(nameof(TransactionId), IsUnique = true)]
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        // =========================
        // RELATION → SHIPMENT
        // =========================
        [Required]
        public int ShipmentId { get; set; }

        [ForeignKey(nameof(ShipmentId))]
        public Shipment Shipment { get; set; } = null!;
        public int? OrderId { get; set; }
        
        // =========================
        // MONEY
        // =========================
        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();
        [Required, StringLength(3)]
        public string Currency { get; set; } = "EUR";
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        // 👇 FK
        public int? ProcessedByEmployeeId { get; set; }

        // 👇 Navigation
        [ForeignKey(nameof(ProcessedByEmployeeId))]
        public Employee? ProcessedByEmployee { get; set; }
        // =========================
        // PROVIDER / METHOD
        // =========================
        [Required, StringLength(50)]
        public string Provider { get; set; } = "Stripe";

        [Required, StringLength(50)]
        public string PaymentMethod { get; set; } = "Auto";
        // Card / ApplePay / GooglePay / SEPA

        // =========================
        // STATUS
        // =========================
        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public ICollection<CardTransaction> CardTransactions { get; set; } = new List<CardTransaction>();
        // =========================
        // STRIPE IDS
        // =========================
        [StringLength(100)]
        public string? ExternalPaymentId { get; set; } // pi_...

        [StringLength(100)]
        public string? TransactionId { get; set; } // charge / capture

        // =========================
        // AUDIT
        // =========================
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }
}

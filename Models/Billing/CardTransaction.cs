using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Shipping;

namespace SecureTransparentDataExchange.Models.Billing
{
    public class CardTransaction
    {
        [Key]
        public int Id { get; set; }

        // =========================
        // LINK TO SHIPMENT
        // =========================
        [Required]
        public int ShipmentId { get; set; }

        [ForeignKey(nameof(ShipmentId))]
        public Shipment Shipment { get; set; } = null!;

        // =========================
        // PAYMENT INFO
        // =========================
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(25)]
        public string CardNumberMasked { get; set; } = string.Empty;
        // e.g. **** **** **** 4242
        public ICollection<CardTransaction> CardTransactions { get; set; } = new List<CardTransaction>();
        [StringLength(20)]
        public string CardBrand { get; set; } = string.Empty;
        // Visa, Mastercard, Amex (from Stripe)

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // =========================
        // STRIPE / PAYMENT PROVIDER
        // =========================
        [StringLength(100)]
        public string Provider { get; set; } = "Stripe";

        [StringLength(100)]
        public string ProviderTransactionId { get; set; } = string.Empty;
        // payment_intent.id / charge.id

        [StringLength(200)]
        public string? ProviderMessage { get; set; }

        // =========================
        // AUDIT
        // =========================
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }
}

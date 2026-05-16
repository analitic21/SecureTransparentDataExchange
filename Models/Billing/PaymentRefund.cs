using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Models.Billing
{
    public class PaymentRefund
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; } = null!;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "EUR";

        [MaxLength(100)]
        public string? StripeRefundId { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
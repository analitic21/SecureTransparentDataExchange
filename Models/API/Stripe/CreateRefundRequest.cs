using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.API.Stripe
{
    public class CreateRefundRequest
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
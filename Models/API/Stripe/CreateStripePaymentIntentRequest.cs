using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.API.Stripe
{
    public class CreateStripePaymentIntentRequest
    {
        [Required]
        public int ShipmentId { get; set; }

        public decimal? Amount { get; set; }
    }

    public class CreateStripePaymentIntentResponse
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
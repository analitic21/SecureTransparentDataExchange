using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.DTOs
{
    public class CreateShipmentRequest
    {
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public int? ClientId { get; set; }
    }
}
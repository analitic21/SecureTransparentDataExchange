// CreateParcelRequest.cs
namespace SecureTransparentDataExchange.DTOs
{
    public class CreateParcelRequest
    {
        public string? TrackingNumber { get; set; }

        public string? Sender { get; set; }

        public string? Receiver { get; set; }

        public string? Status { get; set; }

        public int? ShipmentId { get; set; }

        public int? CargoTypeId { get; set; }
    }
}
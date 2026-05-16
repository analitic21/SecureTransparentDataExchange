namespace SecureTransparentDataExchange.DTOs
{
    public class UpdateParcelRequest
    {
        public string? Sender { get; set; }

        public string? Receiver { get; set; }

        public string? Status { get; set; }

        public int? ShipmentId { get; set; }

        public int? CargoTypeId { get; set; }
    }
}
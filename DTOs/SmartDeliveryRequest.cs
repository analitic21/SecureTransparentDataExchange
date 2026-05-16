namespace SecureTransparentDataExchange.DTOs
{
    public class SmartDeliveryRequest
    {
        public int ParcelId { get; set; }
        public List<List<int>> Distances { get; set; } = new();
    }
}
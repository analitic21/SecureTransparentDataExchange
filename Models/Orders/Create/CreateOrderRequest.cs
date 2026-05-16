namespace SecureTransparentDataExchange.Models.Orders.Create
{
    public class CreateOrderRequest
    {
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string? ExternalOrderNumber { get; set; }
        public string Source { get; set; } = "Web";
    }
}
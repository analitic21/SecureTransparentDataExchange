using System;

namespace SecureTransparentDataExchange.Models
{
    public class DeliveryPriceResponse
    {
        public bool Success { get; set; } = true; // Default to true
        public string? Message { get; set; }
        public double DistanceInKm { get; set; }
        public decimal Price { get; set; }
        public TimeSpan EstimatedDeliveryTime { get; set; }
    }
}

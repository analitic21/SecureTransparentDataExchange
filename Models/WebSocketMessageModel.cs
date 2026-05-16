using System;

namespace SecureTransparentDataExchange.Models
{
    public class WebSocketMessageModel
    {
        public string Type { get; set; } = "notification";
        public string User { get; set; } = "system";
        public string Message { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string? ShipmentStatus { get; set; }
    }
}
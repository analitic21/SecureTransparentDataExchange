using System;

namespace SecureTransparentDataExchange.Models
{
    public class TrackingNumberRequest
    {
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string QrCodeBase64 { get; set; }
        public required string UserId { get; set; } // Added UserId property 
    }
}
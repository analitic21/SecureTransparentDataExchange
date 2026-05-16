using System;

namespace SecureTransparentDataExchange.Models
{
    public class RandomNumberGeneratorModel
    {
        public int Size { get; set; }
        public string Token { get; set; } = string.Empty; // Initialize with empty string
        public string? RandomBytes { get; set; } // Nullable property
    }
}
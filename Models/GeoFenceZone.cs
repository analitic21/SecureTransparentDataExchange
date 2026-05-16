using System;
using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class GeoFenceZone
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public double CenterLat { get; set; }
        public double CenterLng { get; set; }

        public int RadiusMeters { get; set; }

        public int? EncryptionKeyId { get; set; }
        public EncryptionKey? EncryptionKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
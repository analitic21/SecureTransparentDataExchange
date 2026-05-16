using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.DTOs
{
    public class GeoFenceZoneDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public double CenterLat { get; set; }

        [Required]
        [Range(-180, 180)]
        public double CenterLng { get; set; }

        [Required]
        [Range(1, 100000)] // до 100 км
        public int RadiusMeters { get; set; }

        // 🔐 опционально (если используешь шифрование)
        public int? EncryptionKeyId { get; set; }
    }
}
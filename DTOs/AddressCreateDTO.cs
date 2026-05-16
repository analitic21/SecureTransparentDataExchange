using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.DTOs
{
    public class AddressCreateDTO
    {
        [Required]
        [StringLength(255)]
        public string Street { get; set; } = null!;

        [Required]
        public int PostalCodeId { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
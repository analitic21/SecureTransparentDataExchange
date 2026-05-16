using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureTransparentDataExchange.Models.Location
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string Street { get; set; } = string.Empty;

        [Required]
        public int PostalCodeId { get; set; }

        [ForeignKey(nameof(PostalCodeId))]
        public PostalCode PostalCode { get; set; } = null!;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
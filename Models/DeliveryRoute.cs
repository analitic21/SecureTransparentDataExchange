using SecureTransparentDataExchange.Models.Location;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureTransparentDataExchange.Models
{
    public class DeliveryRoute
    {
        [Key]
        public int Id { get; set; }

        // Data to be sent (from) 
        [Required]
        public int OriginCountryId { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        [ForeignKey(nameof(OriginCountryId))]
        public virtual Country OriginCountry { get; set; } = null!;

        [Required]
        public int OriginCityId { get; set; }

        [ForeignKey(nameof(OriginCityId))]
        public virtual City OriginCity { get; set; } = null!;

        [Required]
        public int OriginPostalCodeId { get; set; }

        [ForeignKey(nameof(OriginPostalCodeId))]
        public virtual PostalCode OriginPostalCode { get; set; } = null!;

        // Data for delivery (where) 
        [Required]
        public int DestinationCountryId { get; set; }

        [ForeignKey(nameof(DestinationCountryId))]
        public virtual Country DestinationCountry { get; set; } = null!;

        [Required]
        public int DestinationCityId { get; set; }

        [ForeignKey(nameof(DestinationCityId))]
        public virtual City DestinationCity { get; set; } = null!;

        [Required]
        public int DestinationPostalCodeId { get; set; }

        [ForeignKey(nameof(DestinationPostalCodeId))]
        public virtual PostalCode DestinationPostalCode { get; set; } = null!;
    }
}
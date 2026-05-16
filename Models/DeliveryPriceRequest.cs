using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class DeliveryPriceRequest
    {
        [Required]
        public int OriginAddressId { get; set; }

        [Required]
        public int DestinationAddressId { get; set; }
    }
}

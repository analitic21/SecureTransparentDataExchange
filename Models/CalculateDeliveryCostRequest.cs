using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class CalculateDeliveryCostRequest
    {
        [Required]
        public DemandForecastModel ForecastModel { get; set; } = null!;

        [Required]
        public int OriginAddressId { get; set; }

        [Required]
        public int DestinationAddressId { get; set; }
    }
}

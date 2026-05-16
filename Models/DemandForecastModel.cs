using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class DemandForecastModel
    {
        public DateTime ForecastDate { get; set; }

        [Range(0, int.MaxValue)]
        public int ShipmentCount { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class ForecastPrediction
    {
        [Key]
        public int Id { get; set; }

        // Let's make PredictedOrders nullable 
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Predicted orders cannot be negative.")]
        public int? PredictedOrders { get; set; } // Now it can be null 

        [Required]
        [DataType(DataType.Date)]
        public DateTime ForecastDate { get; set; }

        [Required]
        public string Description { get; set; } = "Forecast result";
    }
}
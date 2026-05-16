using System;

namespace SecureTransparentDataExchange.Models
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        // Add the required modifier so the property cannot be null
        public required string Summary { get; set; }
    }
}
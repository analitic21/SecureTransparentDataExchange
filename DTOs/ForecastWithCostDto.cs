namespace SecureTransparentDataExchange.DTOs
{
    public class ForecastWithCostDto
    {
        public int Day { get; set; }
        public float PredictedShipments { get; set; }
        public decimal TotalCost { get; set; }
    }
}
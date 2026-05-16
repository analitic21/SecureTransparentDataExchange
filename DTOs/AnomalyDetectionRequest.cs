namespace SecureTransparentDataExchange.DTOs
{
    public class AnomalyDetectionRequest
    {
        public float Feature1 { get; set; }
        public float Feature2 { get; set; }
        public float Feature3 { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
namespace SecureTransparentDataExchange.DTOs
{
    public class AnomalyDetectionResult
    {
        public int Index { get; set; }
        public bool IsAnomaly { get; set; }
        public float Score { get; set; }
        public DateTime Timestamp { get; set; }

        public float Feature1 { get; set; }
        public float Feature2 { get; set; }
        public float Feature3 { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
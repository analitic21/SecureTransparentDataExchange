using Microsoft.ML.Data;

namespace SecureTransparentDataExchange.Models.ML
{
    public class ForecastPredictionML
    {
        [VectorType(12)]
        public float[] PredictedOrders { get; set; } = Array.Empty<float>();
    }
}
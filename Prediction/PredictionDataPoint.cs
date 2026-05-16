using Microsoft.ML.Data;

namespace SecureTransparentDataExchange.AI.Prediction
{
    public class PredictionDataPoint
    {
        [LoadColumn(0)]
        public float Feature1 { get; set; }

        [LoadColumn(1)]
        public float Feature2 { get; set; }

        [LoadColumn(2)]
        public float Feature3 { get; set; }
    }
}

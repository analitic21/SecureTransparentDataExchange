using Microsoft.ML;
using Microsoft.ML.Data;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;

namespace SecureTransparentDataExchange.AI.Prediction
{
    public class AnomalyPredictionService
    {
        private static readonly MLContext _mlContext = new MLContext(seed: 1);

        public List<AnomalyDetectionResult> Predict(List<AnomalyDetectionRequest> request)
        {
            if (request == null || request.Count == 0)
                throw new ArgumentException("Input data cannot be empty.");

            var data = request.Select(x => new DataPointForML
            {
                Features = new float[]
                {
                    x.Feature1,
                    x.Feature2,
                    x.Feature3
                },
                Timestamp = x.Timestamp == default ? DateTime.UtcNow : x.Timestamp
            }).ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.AnomalyDetection.Trainers.RandomizedPca(
                featureColumnName: nameof(DataPointForML.Features),
                rank: 2
            );

            var model = pipeline.Fit(dataView);
            var transformed = model.Transform(dataView);

            var rawPredictions = _mlContext.Data
                .CreateEnumerable<AnomalyPredictionRawResult>(
                    transformed,
                    reuseRowObject: false
                )
                .ToList();

            return rawPredictions.Select((p, index) => new AnomalyDetectionResult
            {
                Index = index,
                IsAnomaly = p.IsAnomaly,
                Score = p.Score,
                Timestamp = data[index].Timestamp,
                Feature1 = request[index].Feature1,
                Feature2 = request[index].Feature2,
                Feature3 = request[index].Feature3,
                Message = p.IsAnomaly
                    ? "Potential anomaly predicted"
                    : "Normal data point"
            }).ToList();
        }
    }

    public class AnomalyPredictionRawResult
    {
        [ColumnName("PredictedLabel")]
        public bool IsAnomaly { get; set; }

        [ColumnName("Score")]
        public float Score { get; set; }
    }
}
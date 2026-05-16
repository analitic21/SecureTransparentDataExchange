using Microsoft.ML; // Required using directive
using System;
using System.IO;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Models;
using Microsoft.ML.Data;

namespace SecureTransparentDataExchange.Services
{
    public class PipelineTrainingService
    {
        private readonly MLContext _mlContext;
        private readonly string _modelPath = "Models/DemandForecastModel.zip";

        public PipelineTrainingService()
        {
            _mlContext = new MLContext(); // Initialize MLContext 
        }

        public async Task TrainModelAsync(string dataPath)
        {
            if (!File.Exists(dataPath))
                throw new FileNotFoundException($"Data not found at path: {dataPath}");

            var dataView = await Task.Run(() =>
            _mlContext.Data.LoadFromTextFile<DemandForecastModel>(dataPath, hasHeader: true, separatorChar: ',')
            );

            var pipeline = _mlContext.Forecasting.ForecastBySsa(
            outputColumnName: nameof(ForecastPrediction.PredictedOrders),
            inputColumnName: nameof(DemandForecastModel.ShipmentCount),
            windowSize: 12,
            seriesLength: 24,
            trainSize: 36,
            horizon: 12
            );

            var model = pipeline.Fit(dataView);

            _mlContext.Model.Save(model, dataView.Schema, _modelPath);
        }
    }
}
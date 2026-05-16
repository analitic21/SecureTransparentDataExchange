using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models.ML;

namespace SecureTransparentDataExchange.Services
{
    public class DemandForecastService
    {
        private readonly MLContext _mlContext;
        private readonly DeliveryPriceService _deliveryPriceService;
        private readonly string _modelPath = Path.Combine("Models", "DemandForecastModel.zip");

        public DemandForecastService(DeliveryPriceService deliveryPriceService)
        {
            _mlContext = new MLContext(seed: 1);
            _deliveryPriceService = deliveryPriceService;
        }

        // 🔥 ML Prediction 
        public Task<List<float>> PredictDemandAsync(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Days must be greater than 0.", nameof(days));

            if (!File.Exists(_modelPath))
                throw new FileNotFoundException("Model not trained", _modelPath);

            var model = _mlContext.Model.Load(_modelPath, out _);

            var engine = model.CreateTimeSeriesEngine<DemandForecastModel, ForecastPredictionML>(_mlContext);

            var forecast = engine.Predict();

            var result = forecast.PredictedOrders
                .Take(days)
                .Select(x => Math.Max(0, x))
                .ToList();

            return Task.FromResult(result);
        }
        public async Task<List<ForecastWithCostDto>> PredictWithCostAsync(
    int days,
    int originAddressId,
    int destinationAddressId)
        {
            var predictions = await PredictDemandAsync(days);

            var result = new List<ForecastWithCostDto>();

            foreach (var (value, index) in predictions.Select((v, i) => (v, i)))
            {
                var shipments = (int)value;

                var cost = await CalculateForecastCostAsync(
                    originAddressId,
                    destinationAddressId,
                    shipments
                );

                result.Add(new ForecastWithCostDto
                {
                    Day = index + 1,
                    PredictedShipments = value,
                    TotalCost = cost
                });
            }

            return result;
        }
        // 🔥 Communication with business logic (cost) 
        public async Task<decimal> CalculateForecastCostAsync(
        int originAddressId,
        int destinationAddressId,
        int predictedShipments)
        {
            var deliveryResponse = await _deliveryPriceService.CalculateDeliveryPriceAsync(
            originAddressId,
            destinationAddressId
            );

            if (!deliveryResponse.Success)
                throw new InvalidOperationException(deliveryResponse.Message);

            return deliveryResponse.Price * predictedShipments;
        }
    }
}
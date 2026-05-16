using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using  SecureTransparentDataExchange.DTOs;
using Microsoft.Extensions.Logging;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemandForecastController : ControllerBase
    {
        private readonly PipelineTrainingService _pipelineTrainingService;
        private readonly DemandForecastService _forecastService;
        private readonly ILogger<DemandForecastController> _logger;

        public DemandForecastController(
            PipelineTrainingService pipelineTrainingService,
            DemandForecastService forecastService,
            ILogger<DemandForecastController> logger)
        {
            _pipelineTrainingService = pipelineTrainingService;
            _forecastService = forecastService;
            _logger = logger;
        }

        // 🔥 TRAIN MODEL
        [HttpPost("train")]
        public async Task<IActionResult> TrainModel([FromBody] List<DemandForecastModel> data)
        {
            if (data == null || data.Count == 0)
                return BadRequest(new { message = "No data provided" });

            try
            {
                var path = await SaveDataToCsvAsync(data);
                await _pipelineTrainingService.TrainModelAsync(path);

                return Ok(new
                {
                    success = true,
                    message = "Model trained"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Training failed");

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // 🔥 FULL FORECAST + COST
        [HttpGet("full")]
        public async Task<IActionResult> GetFullForecast(
            [FromQuery] int days,
            [FromQuery] int originId,
            [FromQuery] int destinationId)
        {
            if (days <= 0)
                return BadRequest(new { message = "Days must be > 0" });

            try
            {
                var data = await _forecastService.PredictWithCostAsync(
                    days,
                    originId,
                    destinationId
                );

                return Ok(new
                {
                    success = true,
                    days,
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forecast failed");

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // 🔥 ONLY FORECAST (без цены)
        [HttpGet("predict/{days:int}")]
        public async Task<IActionResult> Predict(int days)
        {
            if (days <= 0)
                return BadRequest(new { message = "Days must be > 0" });

            try
            {
                var result = await _forecastService.PredictDemandAsync(days);

                return Ok(new
                {
                    success = true,
                    predictions = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Prediction failed");

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // 🔥 CSV SAVE
        private static async Task<string> SaveDataToCsvAsync(List<DemandForecastModel> data)
        {
            var fileName = $"forecast_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var path = Path.Combine(Path.GetTempPath(), fileName);

            await using var writer = new StreamWriter(path);

            await writer.WriteLineAsync("ForecastDate,ShipmentCount");

            foreach (var item in data)
            {
                var line = $"{item.ForecastDate:yyyy-MM-dd},{item.ShipmentCount}";
                await writer.WriteLineAsync(line);
            }

            return path;
        }
    }
}
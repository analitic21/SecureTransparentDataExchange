using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.AI.Prediction;
using  SecureTransparentDataExchange.DTOs;


namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnomalyPredictionController : ControllerBase
    {
        private readonly AnomalyPredictionService _service;
        private readonly ILogger<AnomalyPredictionController> _logger;

        public AnomalyPredictionController(
            AnomalyPredictionService service,
            ILogger<AnomalyPredictionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Administrator")]
        [HttpPost("predict-anomalies")]
        public IActionResult PredictAnomalies([FromBody] List<AnomalyDetectionRequest> request)
        {
            if (request == null || request.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Dataset is empty."
                });
            }

            try
            {
                var result = _service.Predict(request);

                return Ok(new
                {
                    success = true,
                    count = result.Count,
                    predictions = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during anomaly prediction.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while predicting anomalies.",
                    error = ex.Message
                });
            }
        }
    }
}
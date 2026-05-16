using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.AI.Detection;
using  SecureTransparentDataExchange.DTOs;

namespace SecureTransparentDataExchange.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnomalyDetectionController : ControllerBase
{
    private readonly AnomalyDetectionService _service;
    private readonly ILogger<AnomalyDetectionController> _logger;

    public AnomalyDetectionController(
        AnomalyDetectionService service,
        ILogger<AnomalyDetectionController> logger)
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
                message = "Dataset is empty"
            });
        }

        try
        {
            var result = _service.Detect(request);

            return Ok(new
            {
                success = true,
                count = result.Count,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Anomaly detection failed");

            return StatusCode(500, new
            {
                success = false,
                message = "Anomaly detection error",
                details = ex.Message
            });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")] // Now the URL will use the controller name
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        // Inject ILogger<ApiController> into the constructor
        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Example of GET method for testing the connection
        [HttpGet("test")]
        public IActionResult GetTest()
        {
            _logger.LogInformation("GET test method called");
            var data = new { message = "Hello from ApiController!" };
            return Ok(data);
        }

        // Example of GET method for getting the API status
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            _logger.LogInformation("GET status method called");
            var status = new { status = "API is working", timestamp = DateTime.UtcNow };
            return Ok(status);
        }

        // Example of GET method with parameter
        [HttpGet("getbyid/{id}")]
        public IActionResult GetById(string id)
        {
            _logger.LogInformation($"GET getbyid method called with id: {id}");

            if (int.TryParse(id, out int numericId))
            {
                var data = new { message = $"You requested numeric ID: {numericId}" };
                return Ok(data);
            }
            else
            {
                _logger.LogWarning($"Invalid ID format received: {id}");
                return BadRequest(new { error = "Invalid ID format", requestedId = id });
            }
        }

        // Example of POST method for testing data transfer
        [HttpPost("echo")]
        public IActionResult Echo([FromBody] dynamic payload)
        {
            _logger.LogInformation($"POST echo method called with payload: {payload}");
            return Ok(new { received = payload });
        }

        // Example of PUT method for updating data
        [HttpPut("update/{id}")]
        public IActionResult Update(string id, [FromBody] dynamic payload)
        {
            _logger.LogInformation($"PUT update method called with id: {id} and payload: {payload}");
            return Ok(new { message = $"Updated record with ID: {id}", updatedData = payload });
        }

        // Example of DELETE method for deleting data
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(string id)
        {
            _logger.LogInformation($"DELETE method called for ID: {id}");
            return Ok(new { message = $"Deleted record with ID: {id}" });
        }
    }
}

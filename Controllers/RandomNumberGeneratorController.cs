using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController] // Make sure the attribute is only used once 
    public class RandomNumberGeneratorController : ControllerBase
    {
        private readonly RandomNumberGeneratorService _randomNumberGeneratorService;
        private readonly ILogger<RandomNumberGeneratorController> _logger;

        // Use a single constructor with proper initialization
        public RandomNumberGeneratorController(RandomNumberGeneratorService randomNumberGeneratorService, ILogger<RandomNumberGeneratorController> logger)
        {
            _randomNumberGeneratorService = randomNumberGeneratorService ?? throw new ArgumentNullException(nameof(randomNumberGeneratorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a random token and random bytes.
        /// </summary>
        /// <param name="size">Size of token or byte array (default is 32 bytes).</param>
        /// <returns>Result with token and bytes in model.</returns>
        [HttpPost("generate")]
        public IActionResult GenerateRandomData([FromQuery] int size = 32)
        {
            try
            {
                var result = _randomNumberGeneratorService.GenerateRandomData(size); // Get the result

                return Ok(new
                {
                    success = true,
                    message = "Random data generated successfully",
                    data = result // Return the model with the results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating random data.");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }
    }
}
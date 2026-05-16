using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admin can execute seeding manually
    public class SeedDataController : ControllerBase
    {
        private readonly IServiceProvider _services;
        private readonly IHostEnvironment _env;
        private readonly ILogger<SeedDataController> _logger;

        public SeedDataController(
            IServiceProvider services,
            IHostEnvironment env,
            ILogger<SeedDataController> logger)
        {
            _services = services;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Initializes database with global location & default data.
        /// Available only in Development environment.
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> InitializeData()
        {
            if (!_env.IsDevelopment())
            {
                _logger.LogWarning("❌ Seed endpoint blocked outside Development environment.");
                return Forbid();
            }

            try
            {
                await SeedData.InitializeAsync(_services);
                return Ok(new { message = "✔ Database seeded successfully with global data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing database.");
                return StatusCode(500, new { message = $"Error initializing database: {ex.Message}" });
            }
        }
    }
}

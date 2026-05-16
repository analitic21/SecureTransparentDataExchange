using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Models;
using Microsoft.AspNetCore.Mvc;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private readonly WeatherForecastService _weatherForecastService;

        public WeatherForecastController(WeatherForecastService weatherForecastService)
        {
            _weatherForecastService = weatherForecastService;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return _weatherForecastService.GetForecast();
        }
    }
}

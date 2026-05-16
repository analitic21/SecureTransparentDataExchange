using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services.Realtime;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteOptimizationController : ControllerBase
    {
        private readonly RouteService _routeService;

        public RouteOptimizationController(RouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpPost("optimize")]
        public IActionResult Optimize([FromBody] RouteDataDTO model)
        {
            if (model?.Distances == null || model.Distances.Count == 0)
                return BadRequest("Invalid data: Distance matrix is required.");

            int size = model.Distances.Count;
            int[,] matrix = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                if (model.Distances[i].Count != size)
                    return BadRequest("Matrix must be square.");

                for (int j = 0; j < size; j++)
                    matrix[i, j] = model.Distances[i][j];
            }

            var optimizedRoute = _routeService.GetOptimizedRoute(matrix);

            return Ok(new
            {
                OptimizedRoute = optimizedRoute.Route,
                TotalDistance = optimizedRoute.TotalDistance
            });
        }
    }
}

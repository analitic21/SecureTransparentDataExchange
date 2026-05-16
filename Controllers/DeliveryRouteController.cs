using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Administrator")]
    public class DeliveryRouteController : ControllerBase
    {
        private readonly DeliveryRouteService _routeService;
        private readonly ILogger<DeliveryRouteController> _logger;

        public DeliveryRouteController(
            DeliveryRouteService routeService,
            ILogger<DeliveryRouteController> logger)
        {
            _routeService = routeService;
            _logger = logger;
        }
        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            return Ok(routes);
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRouteById(int id)
        {
            var route = await _routeService.GetRouteByIdAsync(id);
            if (route == null)
                return NotFound(new { message = $"Route {id} not found" });

            return Ok(route);
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] DeliveryRoute route)
        {
            if (route == null)
                return BadRequest("Route is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _routeService.CreateRouteAsync(route);

            _logger.LogInformation("Route created: {Id}", created.Id);

            return CreatedAtAction(nameof(GetRouteById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] DeliveryRoute route)
        {
            if (route == null)
                return BadRequest("Route is required.");

            if (id != route.Id)
                return BadRequest("Route ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _routeService.UpdateRouteAsync(id, route);

            if (updated == null)
                return NotFound(new { message = $"Route {id} not found" });

            return Ok(updated);
        }

        [Authorize(Roles = "Admin,Administrator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var success = await _routeService.DeleteRouteAsync(id);

            if (!success)
                return NotFound(new { message = $"Route {id} not found" });

            return NoContent();
        }
    }
}
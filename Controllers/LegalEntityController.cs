using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.API.LegalEntities;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Models.Orders;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔐 JWT REQUIRED
    public class LegalEntityController : ControllerBase
    {
        private readonly ILegalEntityService _service;
        private readonly ILogger<LegalEntityController> _logger;

        public LegalEntityController(
            ILegalEntityService service,
            ILogger<LegalEntityController> logger)
        {
            _service = service;
            _logger = logger;
        }
        // =====================================
        // GET ALL LEGAL ENTITIES (ADMIN)
        // =====================================
        [Authorize(Roles = "Admin,Administrator")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            try
            {
                var entities = await _service.GetAllAsync(ct);
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching legal entities");
                return StatusCode(500, new { message = "Error fetching legal entities." });
            }
        }
        // =====================================
        // GET MY LEGAL ENTITY (🔥 IMPORTANT)
        // =====================================
        [HttpGet("me")]
        public async Task<IActionResult> GetMy(CancellationToken ct)
        {
            try
            {
                var userId = GetUserId();
                if (userId <= 0)
                    return Unauthorized(new { message = "Invalid user." });

                var entity = await _service.GetByLoginIdAsync(userId, ct);

                if (entity == null)
                    return NotFound(new { message = "Legal entity not found." });

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching my legal entity");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // =====================================
        // CREATE (any authorized)
        // =====================================
        // CREATE
        [EnableRateLimiting("api-write")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromBody] LegalEntityCreateDto dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();
                if (userId <= 0)
                    return Unauthorized();

                var created = await _service.CreateAsync(dto, userId, ct);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating legal entity");
                return StatusCode(500, new { message = "Create failed." });
            }
        }

        // =====================================
        // UPDATE (ONLY YOURS)
        // =====================================
        // UPDATE
        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] LegalEntityUpdateDto dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();

                var updated = await _service.UpdateAsync(dto, userId, ct);

                if (updated == null)
                    return NotFound(new { message = "Entity not found or access denied." });

                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating legal entity");
                return StatusCode(500, new { message = "Update failed." });
            }
        }


        // =====================================
        // DELETE (ADMIN ONLY)
        // =====================================
        [Authorize(Roles = "Admin,Administrator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                var ok = await _service.DeleteAsync(id, ct);

                if (!ok)
                    return NotFound(new { message = "Not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting legal entity");
                return StatusCode(500, new { message = "Delete failed." });
            }
        }

        // =====================================
        // GET BY ID (ADMIN)
        // =====================================
        [Authorize(Roles = "Admin,Administrator")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id, ct);

                if (entity == null)
                    return NotFound();

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity");
                return StatusCode(500, new { message = "Error fetching entity." });
            }
        }

        // =====================================
        // JWT HELPER
        // =====================================
        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            return claim != null && int.TryParse(claim.Value, out var id)
                ? id
                : 0;
        }
    }
}
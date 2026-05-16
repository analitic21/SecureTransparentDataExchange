using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Administrator")]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(RoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        // =========================
        // GET ALL ROLES
        // =========================
        [HttpGet]
        public async Task<ActionResult<List<Role>>> GetRoles()
        {
            try
            {
                var roles = await _roleService.GetRolesAsync();

                if (roles == null || roles.Count == 0)
                    return NotFound(new { message = "No roles found." });

                return Ok(roles);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching roles.");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // =========================
        // GET ROLE BY ID
        // =========================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);

                if (role == null)
                    return NotFound(new { message = $"Role with ID {id} not found." });

                return Ok(role);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching role with ID {RoleId}.", id);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // =========================
        // CREATE ROLE
        // =========================
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] Role role)
        {
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
                return BadRequest(new { message = "Role data is required and must have a valid name." });

            try
            {
                role.Name = role.Name.Trim();

                var existingRole = await _roleService.GetRoleByNameAsync(role.Name);
                if (existingRole != null)
                    return Conflict(new { message = $"Role with name '{role.Name}' already exists." });

                await _roleService.AddRoleAsync(role);

                return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding new role.");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // =========================
        // UPDATE ROLE
        // =========================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
                return BadRequest(new { message = "Invalid role data." });

            if (id != role.Id)
                return BadRequest(new { message = "Route ID does not match role ID." });

            try
            {
                var existingRole = await _roleService.GetRoleByIdAsync(id);
                if (existingRole == null)
                    return NotFound(new { message = $"Role with ID {id} not found." });

                role.Name = role.Name.Trim();

                var roleWithSameName = await _roleService.GetRoleByNameAsync(role.Name);
                if (roleWithSameName != null && roleWithSameName.Id != id)
                    return Conflict(new { message = $"Role with name '{role.Name}' already exists." });

                await _roleService.UpdateRoleAsync(id, role);

                return Ok(new { message = "Role updated successfully." });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating role with ID {RoleId}.", id);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // =========================
        // DELETE ROLE
        // =========================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var existingRole = await _roleService.GetRoleByIdAsync(id);
                if (existingRole == null)
                    return NotFound(new { message = $"Role with ID {id} not found." });

                await _roleService.DeleteRoleAsync(id);

                return Ok(new { message = "Role deleted successfully." });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting role with ID {RoleId}.", id);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
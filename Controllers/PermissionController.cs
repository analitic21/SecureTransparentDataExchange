using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly PermissionService _permissionService;

        public PermissionController(PermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        // Get all permissions
        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _permissionService.GetPermissionsAsync();
            if (permissions == null)
            {
                return NotFound(new { message = "No permissions found." });
            }

            return Ok(new { message = "Permissions retrieved successfully.", data = permissions });
        }

        // Get permission by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound(new { message = $"Permission with ID {id} not found." });
            }

            return Ok(new { message = "Permission retrieved successfully.", data = permission });
        }

        // Add a new permission
        [HttpPost]
        public async Task<IActionResult> AddPermission([FromBody] Permission permission)
        {
            var addedPermission = await _permissionService.AddPermissionAsync(permission);
            if (addedPermission == null)
            {
                return BadRequest(new { message = "Permission cannot be null or an error occurred while adding it." });
            }

            return Ok(new { message = "Permission added successfully.", data = addedPermission });
        }

        // Update an existing permission
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] Permission permission)
        {
            var updatedPermission = await _permissionService.UpdatePermissionAsync(id, permission);
            if (updatedPermission == null)
            {
                return NotFound(new { message = $"Permission with ID {id} not found or error occurred." });
            }

            return Ok(new { message = "Permission updated successfully.", data = updatedPermission });
        }

        // Delete a permission
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var deletedPermission = await _permissionService.DeletePermissionAsync(id);
            if (deletedPermission == null)
            {
                return NotFound(new { message = $"Permission with ID {id} not found." });
            }

            return Ok(new { message = "Permission deleted successfully.", data = deletedPermission });
        }
    }
}

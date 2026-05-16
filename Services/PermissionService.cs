using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Services
{
    public class PermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(ApplicationDbContext context, ILogger<PermissionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Get all permissions
        public async Task<ActionResult<List<Permission>>> GetPermissionsAsync()
        {
            try
            {
                var permissions = await _context.Permissions.ToListAsync();
                if (permissions == null || !permissions.Any())
                {
                    _logger.LogWarning("No permissions found.");
                    return new ActionResult<List<Permission>>(new List<Permission>());
                }

                _logger.LogInformation("Permissions fetched successfully.");
                return new ActionResult<List<Permission>>(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching permissions.");
                return new ActionResult<List<Permission>>(new List<Permission>());
            }
        }

        // Get permission by ID
        public async Task<ActionResult<Permission>> GetPermissionByIdAsync(int id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    _logger.LogWarning($"Permission with ID {id} not found.");
                    return new NotFoundResult();
                }

                _logger.LogInformation($"Permission with ID {id} retrieved successfully.");
                return new ActionResult<Permission>(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching permission with ID {id}.");
                return new NotFoundResult();
            }
        }

        // Add a new permission
        public async Task<ActionResult<Permission>> AddPermissionAsync(Permission permission)
        {
            if (permission == null)
            {
                _logger.LogWarning("Permission cannot be null.");
                return new BadRequestObjectResult("Permission cannot be null.");
            }

            try
            {
                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Permission '{permission.PermissionName}' added successfully.");
                return new ActionResult<Permission>(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding permission.");
                return new StatusCodeResult(500); // Internal server error
            }
        }

        // Update an existing permission
        public async Task<ActionResult<Permission>> UpdatePermissionAsync(int id, Permission permission)
        {
            if (permission == null)
            {
                _logger.LogWarning("Permission cannot be null.");
                return new BadRequestObjectResult("Permission cannot be null.");
            }

            try
            {
                var existingPermission = await _context.Permissions.FindAsync(id);
                if (existingPermission == null)
                {
                    _logger.LogWarning($"Permission with ID {id} not found for update.");
                    return new NotFoundResult();
                }

                existingPermission.PermissionName = permission.PermissionName;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Permission with ID {id} updated successfully.");
                return new ActionResult<Permission>(existingPermission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permission with ID {id}.");
                return new StatusCodeResult(500);
            }
        }

        // Delete a permission
        public async Task<ActionResult<Permission>> DeletePermissionAsync(int id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    _logger.LogWarning($"Permission with ID {id} not found for deletion.");
                    return new NotFoundResult();
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Permission with ID {id} deleted successfully.");
                return new ActionResult<Permission>(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting permission with ID {id}.");
                return new StatusCodeResult(500);
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Requests;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Services.Security;
using SecureTransparentDataExchange.Models.Orders;
using SecureTransparentDataExchange.DTOs;

namespace SecureTransparentDataExchange.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Administrator")]
public class AdminController(AdminService adminService, ILogger<AdminController> logger) : ControllerBase
{
    
    private static class LogTemplates
    {
        public const string NotFound = "{Operation} not found. {Extra}";
        public const string Error = "Error during {Operation}. {Extra}";
        public const string Success = "{Operation} succeeded. {Extra}";
        public const string InvalidData = "Invalid data for {Operation}.";
    }

    // =========================  
    // GET ALL ADMINS  
    // =========================  
    [HttpGet]
    public async Task<ActionResult<List<Admin>>> GetAllAdmins()
    {
        try
        {
            var admins = await adminService.GetAllAdminsAsync();
            if (admins == null || admins.Count == 0)
            {
                logger.LogWarning(LogTemplates.NotFound, "GetAllAdmins", "");
                return NotFound(new { message = "No admins found." });
            }

            logger.LogInformation(LogTemplates.Success, "GetAllAdmins", "");
            return Ok(admins);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "GetAllAdmins", "");
            return StatusCode(500, new { message = "An error occurred while retrieving administrators." });
        }
    }
    [HttpPut("users/{userId:int}/change-role")]
    public async Task<IActionResult> ChangeUserRole(
    int userId,
    [FromBody] ChangeUserRoleRequest request)
    {
        try
        {
            var result = await adminService.ChangeUserRoleAsync(userId, request.RoleId);

            if (!result)
            {
                logger.LogWarning(LogTemplates.NotFound, "ChangeUserRole", $"UserID={userId}, RoleID={request.RoleId}");
                return NotFound(new { message = "User or role not found." });
            }

            logger.LogInformation(LogTemplates.Success, "ChangeUserRole", $"UserID={userId}, RoleID={request.RoleId}");

            return Ok(new
            {
                success = true,
                message = "User role changed successfully."
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "ChangeUserRole", $"UserID={userId}");
            return StatusCode(500, new { message = "Error changing user role." });
        }
    }
    // =========================  
    // GET ALL USERS  
    // =========================  
    [HttpGet("users")]
    public async Task<ActionResult<List<Login>>> GetAllUsers()
    {
        try
        {
            var users = await adminService.GetAllUsersAsync();
            if (users == null || users.Count == 0)
            {
                logger.LogWarning(LogTemplates.NotFound, "GetAllUsers", "");
                return NotFound(new { message = "No users found." });
            }

            logger.LogInformation(LogTemplates.Success, "GetAllUsers", "");
            return Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "GetAllUsers", "");
            return StatusCode(500, new { message = "An error occurred while retrieving users." });
        }
    }

    // =========================  
    // GET ADMIN BY ID  
    // =========================  
    [HttpGet("{id}")]
    public async Task<ActionResult<Admin>> GetAdminById(int id)
    {
        try
        {
            var admin = await adminService.GetAdminByIdAsync(id);
            if (admin == null)
            {
                logger.LogWarning(LogTemplates.NotFound, "GetAdminById", $"ID={id}");
                return NotFound(new { message = $"Admin with ID {id} not found." });
            }

            logger.LogInformation(LogTemplates.Success, "GetAdminById", $"ID={id}");
            return Ok(admin);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "GetAdminById", $"ID={id}");
            return StatusCode(500, new { message = "An error occurred while retrieving the administrator." });
        }
    }

    // =========================  
    // CREATE ADMIN  
    // =========================  
    [HttpPost]
    public async Task<ActionResult<Admin>> CreateAdmin([FromBody] Admin admin)
    {
        if (admin == null)
        {
            logger.LogWarning(LogTemplates.InvalidData, "CreateAdmin");
            return BadRequest(new { message = "Admin data is required." });
        }

        try
        {
            var createdAdmin = await adminService.CreateAdminAsync(admin);
            logger.LogInformation(LogTemplates.Success, "CreateAdmin", $"ID={createdAdmin.AdminId}");

            return CreatedAtAction(nameof(GetAdminById), new { id = createdAdmin.AdminId }, createdAdmin);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "CreateAdmin", "");
            return StatusCode(500, new { message = "An error occurred while creating the administrator." });
        }
    }

    // =========================
    // CHANGE USER LOGIN TYPE
    // =========================
    [HttpPut("users/{userId:int}/change-login-type")]
    public async Task<IActionResult> ChangeUserLoginType(
        int userId,
        [FromBody] ChangeLoginTypeRequest request)
    {
        try
        {
            var result = await adminService.ChangeUserLoginTypeAsync(userId, request.UserType);

            if (!result)
            {
                logger.LogWarning(LogTemplates.NotFound, "ChangeUserLoginType", $"UserID={userId}");
                return NotFound(new { message = "User not found." });
            }

            logger.LogInformation(LogTemplates.Success, "ChangeUserLoginType", $"UserID={userId}");

            return Ok(new
            {
                success = true,
                message = "User login type changed successfully."
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, LogTemplates.InvalidData, "ChangeUserLoginType");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "ChangeUserLoginType", $"UserID={userId}");
            return StatusCode(500, new { message = "Error changing user login type." });
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("iot/devices/{deviceId:int}/token")]
    public async Task<IActionResult> CreateDeviceToken(
        int deviceId,
        [FromServices] DeviceTokenService tokenService,
        [FromServices] ApplicationDbContext db)
    {
        var device = await db.IoTDevices.FindAsync(deviceId);
        if (device == null) return NotFound(new { message = "Device not found" });

        var token = tokenService.GenerateDeviceToken(device.Id, device.TrackingNumber);
        return Ok(new { token });
    }
    // =========================  
    // UPDATE ADMIN  
    // =========================  
    [HttpPut("{id}")]
    public async Task<ActionResult<Admin>> UpdateAdmin(int id, [FromBody] Admin admin)
    {
        if (admin == null)
        {
            logger.LogWarning(LogTemplates.InvalidData, "UpdateAdmin");
            return BadRequest(new { message = "Admin update data is required." });
        }

        try
        {
            var updatedAdmin = await adminService.UpdateAdminAsync(id, admin);
            if (updatedAdmin == null)
            {
                logger.LogWarning(LogTemplates.NotFound, "UpdateAdmin", $"ID={id}");
                return NotFound(new { message = $"Admin with ID {id} not found." });
            }

            logger.LogInformation(LogTemplates.Success, "UpdateAdmin", $"ID={id}");
            return Ok(updatedAdmin);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "UpdateAdmin", $"ID={id}");
            return StatusCode(500, new { message = "An error occurred while updating the administrator." });
        }
    }

    // =========================  
    // DISABLE USER 2FA  
    // =========================  
    [HttpPost("users/{userId}/disable-2fa")]
    public async Task<IActionResult> DisableUserTwoFactor(int userId)
    {
        try
        {
            var result = await adminService.DisableUserTwoFactorAsync(userId);

            if (!result)
            {
                logger.LogWarning(LogTemplates.NotFound, "DisableUserTwoFactor", $"UserID={userId}");
                return NotFound(new { message = "User not found." });
            }

            logger.LogInformation(LogTemplates.Success, "DisableUserTwoFactor", $"UserID={userId}");

            return Ok(new
            {
                success = true,
                message = "Two-factor authentication disabled for user."
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "DisableUserTwoFactor", $"UserID={userId}");
            return StatusCode(500, new { message = "Error disabling 2FA." });
        }
    }

    // =========================  
    // DELETE ADMIN  
    // =========================  
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        try
        {
            var deleted = await adminService.DeleteAdminAsync(id);
            if (!deleted)
            {
                logger.LogWarning(LogTemplates.NotFound, "DeleteAdmin", $"ID={id}");
                return NotFound(new { message = $"Admin with ID {id} not found." });
            }

            logger.LogInformation(LogTemplates.Success, "DeleteAdmin", $"ID={id}");
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "DeleteAdmin", $"ID={id}");
            return StatusCode(500, new { message = "An error occurred while deleting the administrator." });
        }
    }

}
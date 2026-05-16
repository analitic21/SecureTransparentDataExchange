using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Orders;

namespace SecureTransparentDataExchange.Services;

public class AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
{
    private static class LogTemplates
    {
        public const string NotFound = "{Operation} not found. {Extra}";
        public const string Error = "Error during {Operation}. {Extra}";
        public const string Success = "{Operation} succeeded. {Extra}";
        public const string InvalidData = "Invalid data for {Operation}.";
    }

    // ===============================
    // 🔐 NEW: Disable 2FA for user
    // ===============================
    public async Task<bool> DisableUserTwoFactorAsync(int userId)
    {
        try
        {
            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                logger.LogWarning(LogTemplates.NotFound, "DisableUserTwoFactorAsync", $"UserID={userId}");
                return false;
            }

            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecretKey = null;
            user.RecoveryCodes = null;

            await context.SaveChangesAsync();

            logger.LogInformation(LogTemplates.Success, "DisableUserTwoFactorAsync", $"UserID={userId}");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "DisableUserTwoFactorAsync", $"UserID={userId}");
            throw new Exception("Error disabling 2FA for user.", ex);
        }
    }
    public async Task<bool> ChangeUserRoleAsync(int userId, int roleId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
            return false;

        var role = await context.Roles.FindAsync(roleId);
        if (role == null)
            return false;

        user.RoleId = roleId;
        await context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> ChangeUserLoginTypeAsync(int userId, UserType newUserType)
    {
        try
        {
            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                logger.LogWarning(LogTemplates.NotFound, "ChangeUserLoginTypeAsync", $"UserID={userId}");
                return false;
            }

            user.UserType = newUserType;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            logger.LogInformation(
                LogTemplates.Success,
                "ChangeUserLoginTypeAsync",
                $"UserID={userId}, NewType={newUserType}"
            );

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "ChangeUserLoginTypeAsync", $"UserID={userId}");
            throw new Exception("Error changing user login type.", ex);
        }
    }

    // ===============================
    // GET ALL ADMINS
    // ===============================
    public async Task<List<Admin>> GetAllAdminsAsync()
    {
        try
        {
            var result = await context.Admins
                .Include(a => a.User)
                .Include(a => a.Role)
                .ToListAsync();

            logger.LogInformation(LogTemplates.Success, "GetAllAdminsAsync", "");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "GetAllAdminsAsync", "");
            throw new Exception("An error occurred while retrieving the administrators.", ex);
        }
    }

    // ===============================
    // GET ADMIN BY ID
    // ===============================
    public async Task<Admin?> GetAdminByIdAsync(int adminId)
    {
        try
        {
            var admin = await context.Admins
                .Include(a => a.User)
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.AdminId == adminId);

            if (admin == null)
                logger.LogWarning(LogTemplates.NotFound, "GetAdminByIdAsync", $"ID={adminId}");
            else
                logger.LogInformation(LogTemplates.Success, "GetAdminByIdAsync", $"ID={adminId}");

            return admin;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "GetAdminByIdAsync", $"ID={adminId}");
            throw new Exception("An error occurred while retrieving the administrator.", ex);
        }
    }

    // ===============================
    // CREATE ADMIN
    // ===============================
    public async Task<Admin> CreateAdminAsync(Admin admin)
    {
        if (admin == null)
        {
            logger.LogWarning(LogTemplates.InvalidData, "CreateAdminAsync");
            throw new ArgumentNullException(nameof(admin), "Admin data is required.");
        }

        var userExists = await context.Users.AnyAsync(u => u.Id == admin.UserId);
        if (!userExists)
            throw new Exception($"User with ID {admin.UserId} not found.");

        var roleExists = await context.Roles.AnyAsync(r => r.Id == admin.RoleId);
        if (!roleExists)
            throw new Exception($"Role with ID {admin.RoleId} not found.");

        try
        {
            context.Admins.Add(admin);
            await context.SaveChangesAsync();
            logger.LogInformation(LogTemplates.Success, "CreateAdminAsync", $"ID={admin.AdminId}");
            return admin;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, LogTemplates.Error, "CreateAdminAsync", "");
            throw new Exception("An error occurred while saving the admin data.", ex);
        }
    }

    // ===============================
    // UPDATE ADMIN
    // ===============================
    public async Task<Admin?> UpdateAdminAsync(int adminId, Admin updatedAdmin)
    {
        if (updatedAdmin == null)
        {
            logger.LogWarning(LogTemplates.InvalidData, "UpdateAdminAsync");
            throw new ArgumentNullException(nameof(updatedAdmin), "Admin update data is required.");
        }

        var existingAdmin = await context.Admins.FindAsync(adminId);
        if (existingAdmin == null)
        {
            logger.LogWarning(LogTemplates.NotFound, "UpdateAdminAsync", $"ID={adminId}");
            return null;
        }

        var userExists = await context.Users.AnyAsync(u => u.Id == updatedAdmin.UserId);
        if (!userExists)
            throw new Exception($"User with ID {updatedAdmin.UserId} not found.");

        var roleExists = await context.Roles.AnyAsync(r => r.Id == updatedAdmin.RoleId);
        if (!roleExists)
            throw new Exception($"Role with ID {updatedAdmin.RoleId} not found.");

        try
        {
            existingAdmin.UserId = updatedAdmin.UserId;
            existingAdmin.RoleId = updatedAdmin.RoleId;
            existingAdmin.UpdatedAt = DateTime.UtcNow;

            context.Admins.Update(existingAdmin);
            await context.SaveChangesAsync();
            logger.LogInformation(LogTemplates.Success, "UpdateAdminAsync", $"ID={adminId}");
            return existingAdmin;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, LogTemplates.Error, "UpdateAdminAsync", $"ID={adminId}");
            throw new Exception("An error occurred while updating the admin data.", ex);
        }
    }

    // ===============================
    // DELETE ADMIN
    // ===============================
    public async Task<bool> DeleteAdminAsync(int adminId)
    {
        var admin = await context.Admins.FindAsync(adminId);
        if (admin == null)
        {
            logger.LogWarning(LogTemplates.NotFound, "DeleteAdminAsync", $"ID={adminId}");
            return false;
        }

        try
        {
            context.Admins.Remove(admin);
            await context.SaveChangesAsync();
            logger.LogInformation(LogTemplates.Success, "DeleteAdminAsync", $"ID={adminId}");
            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, LogTemplates.Error, "DeleteAdminAsync", $"ID={adminId}");
            throw new Exception("An error occurred while deleting the admin.", ex);
        }
    }

    // ===============================
    // GET ALL USERS
    // ===============================
    public async Task<List<Login>> GetAllUsersAsync()
    {
        try
        {
            var users = await context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .ToListAsync();

            logger.LogInformation(LogTemplates.Success, "GetAllUsersAsync", "");
            return users;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogTemplates.Error, "GetAllUsersAsync", "");
            throw new Exception("An error occurred while retrieving users.", ex);
        }
    }
}
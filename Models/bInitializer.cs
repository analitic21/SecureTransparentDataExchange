using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange
{
    public static class bInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Login>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("bInitializer");

            // 🔄 Apply any pending migrations
            await context.Database.EnsureCreatedAsync(); // Ensure database is created

            // ✅ Create roles
            string[] roleNames = { "Admin", "Client", "Manager" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role
                    {
                        Name = roleName,
                        Description = $"System role: {roleName}"
                    };

                    var result = await roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"✅ Role '{roleName}' created.");
                    }
                    else
                    {
                        logger.LogWarning($"❌ Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // ✅ Create admin user if not exists
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var adminRole = await roleManager.FindByNameAsync("Admin");

                var admin = new Login
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    Name = "Admin",
                    LastName = "User",
                    PhoneNumber = "0000000000",
                    IsEmailConfirmed = true,
                    AgreeToTerms = true,
                    UserType = UserType.Individual,
                    RoleId = adminRole?.Id ?? 1,
                    Role = adminRole!
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    logger.LogInformation("✅ Admin user created and added to 'Admin' role.");
                }
                else
                {
                    logger.LogError($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation("ℹ️ Admin user already exists.");
            }
        }
    }
}

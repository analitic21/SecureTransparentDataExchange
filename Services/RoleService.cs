using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services
{
    public class RoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public RoleService(ApplicationDbContext context, ILogger<RoleService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET ALL ROLES
        public async Task<List<Role>> GetRolesAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        // GET ROLE BY ID
        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // GET ROLE BY NAME
        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return null;

            var normalizedName = roleName.Trim().ToLower();

            var role = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.Name != null &&
                    r.Name.ToLower() == normalizedName);

            if (role == null)
            {
                _logger.LogWarning("Role with name {RoleName} not found.", roleName);
            }

            return role;
        }

        // ADD NEW ROLE
        public async Task AddRoleAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role), "Role cannot be null.");

            if (string.IsNullOrWhiteSpace(role.Name))
                throw new ArgumentException("Role name is required.", nameof(role));

            role.Name = role.Name.Trim();

            var exists = await _context.Roles.AnyAsync(r =>
                r.Name != null &&
                r.Name.ToLower() == role.Name.ToLower());

            if (exists)
                throw new InvalidOperationException($"Role with name '{role.Name}' already exists.");

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        // UPDATE ROLE
        public async Task UpdateRoleAsync(int id, Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role), "Role cannot be null.");

            if (string.IsNullOrWhiteSpace(role.Name))
                throw new ArgumentException("Role name is required.", nameof(role));

            var existingRole = await _context.Roles.FindAsync(id);

            if (existingRole == null)
                throw new KeyNotFoundException($"Role with ID {id} not found.");

            var normalizedName = role.Name.Trim().ToLower();

            var duplicate = await _context.Roles.AnyAsync(r =>
                r.Id != id &&
                r.Name != null &&
                r.Name.ToLower() == normalizedName);

            if (duplicate)
                throw new InvalidOperationException($"Role with name '{role.Name}' already exists.");

            existingRole.Name = role.Name.Trim();
            existingRole.Description = role.Description;

            await _context.SaveChangesAsync();
        }

        // DELETE ROLE
        public async Task DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
                throw new KeyNotFoundException($"Role with ID {id} not found.");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }
    }
}
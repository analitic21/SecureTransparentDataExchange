using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.API;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Services
{
    public class RegistrationService
    {
        private readonly UserManager<Login> _users;
        private readonly RoleManager<Role> _roles;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<RegistrationService> _log;
        private readonly EmailService _email;

        public RegistrationService(
            UserManager<Login> users,
            RoleManager<Role> roles,
            ApplicationDbContext db,
            ILogger<RegistrationService> log,
            EmailService email)
        {
            _users = users;
            _roles = roles;
            _db = db;
            _log = log;
            _email = email;
        }

        public sealed record Result(bool Ok, string Message, int? UserId = null);

        // ======================================================
        // MAIN REGISTRATION METHOD
        // ======================================================
        public async Task<Result> RegisterAsync(RegisterUserRequest req)
        {
            if (req is null)
                return new(false, "Invalid payload.");

            // ------------------------------------------------------
            // Email / username unique
            // ------------------------------------------------------
            if (await _users.FindByEmailAsync(req.Email) != null)
                return new(false, "User with this email already exists.");

            if (await _users.FindByNameAsync(req.Username) != null)
                return new(false, "Username is already taken.");

            // ------------------------------------------------------
            // INDIVIDUAL: Relations Validation
            // ------------------------------------------------------
            if (req.UserType == UserType.Individual)
            {
                var v = await ValidateIndividualRelations(req);
                if (!v.Ok) return v;
            }
            else
            {
                var v = ValidateLegalEntity(req);
                if (!v.Ok) return v;
            }

            // ------------------------------------------------------
            // Ensure User Role
            // ------------------------------------------------------
            var role = await EnsureRoleAsync("User");
            if (role == null)
                return new(false, "Failed to create or resolve default role.");

            // ------------------------------------------------------
            // Create Login model
            // ------------------------------------------------------
            var user = BuildUser(req, role.Id);

            // ------------------------------------------------------
            // Save user
            // ------------------------------------------------------
            var create = await _users.CreateAsync(user, req.Password);
            if (!create.Succeeded)
                return new(false, string.Join("; ", create.Errors.Select(e => e.Description)));

            await _users.AddToRoleAsync(user, "User");

            // ------------------------------------------------------
            // Email notification
            // ------------------------------------------------------
            try
            {
                await _email.SendEmailAsync(
                    user.Email!,
                    "Welcome to Secure Logistics",
                    "You have successfully registered!",
                    null
                );
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to send welcome email.");
            }

            _log.LogInformation("User {Email} registered successfully.", user.Email);
            return new(true, "Registration successful.", user.Id);
        }

        // ======================================================
        // VALIDATION
        // ======================================================
        private async Task<Result> ValidateIndividualRelations(RegisterUserRequest req)
        {
            if (req.AddressId is null)
                return new(false, "Address is required.");

            var addr = await _db.Addresses
                .Include(a => a.PostalCode)
                    .ThenInclude(pc => pc.City)
                        .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(a => a.Id == req.AddressId);

            if (addr == null)
                return new(false, "Address not found.");

            if (addr.PostalCode == null ||
                addr.PostalCode.City == null ||
                addr.PostalCode.City.Country == null)
                return new(false, "Address relations are corrupted.");

            return new(true, "OK");
        }
        private Result ValidateLegalEntity(RegisterUserRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.CompanyName) ||
                string.IsNullOrWhiteSpace(r.TaxId) ||
                string.IsNullOrWhiteSpace(r.RegistrationNumber) ||
                string.IsNullOrWhiteSpace(r.ContactPerson) ||
                string.IsNullOrWhiteSpace(r.ContactPosition) ||
                string.IsNullOrWhiteSpace(r.CompanyPhone) ||
                string.IsNullOrWhiteSpace(r.ManualCity) ||
                string.IsNullOrWhiteSpace(r.ManualCountry) ||
                string.IsNullOrWhiteSpace(r.ManualPostalCode) ||
                string.IsNullOrWhiteSpace(r.ManualAddress))
            {
                return new(false, "All legal entity fields are required.");
            }

            return new(true, "OK");
        }

        // ======================================================
        // BUILD USER
        // ======================================================
        private Login BuildUser(RegisterUserRequest r, int roleId)
        {
            var u = new Login(r.Email)
            {
                UserName = r.Username,
                Email = r.Email,

                Name = r.Name,
                LastName = r.LastName,
                UserType = r.UserType,
                DateOfBirth = r.DateOfBirth,

                AgreeToTerms = r.AgreeToTerms,
                AgreementVersion = r.AgreementVersion ?? "1.0",
                IsEmailConfirmed = false,

                RoleId = roleId
            };

            if (r.UserType == UserType.Individual)
            {
                u.AddressId = r.AddressId;
                u.PhoneNumber = r.PhoneNumber;
            }
            else
            {
                u.CompanyName = r.CompanyName;
                u.TaxId = r.TaxId;
                u.RegistrationNumber = r.RegistrationNumber;
                u.ContactPerson = r.ContactPerson;
                u.ContactPosition = r.ContactPosition;
                u.CompanyPhone = r.CompanyPhone;

                u.ManualCity = r.ManualCity;
                u.ManualCountry = r.ManualCountry;
                u.ManualPostalCode = r.ManualPostalCode;
                u.ManualAddress = r.ManualAddress;

                u.PhoneNumber = r.CompanyPhone;
            }

            return u;
        }

        // ======================================================
        // ROLE HELPERS
        // ======================================================
        private async Task<Role?> EnsureRoleAsync(string name)
        {
            var role = await _roles.FindByNameAsync(name);
            if (role != null) return role;

            role = new Role
            {
                Name = name,
                NormalizedName = name.ToUpperInvariant(),
                Description = "Default user role"
            };

            var res = await _roles.CreateAsync(role);
            return res.Succeeded ? role : null;
        }
    }
}

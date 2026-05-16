using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.API;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Models.identity;


namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/register")]
    public class RegisterController : ControllerBase
    {
        private readonly UserManager<Login> _userManager;
        private readonly ILogger<RegisterController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly RoleManager<Role> _roleManager;

        public RegisterController(
            UserManager<Login> userManager,
            ILogger<RegisterController> logger,
            ApplicationDbContext context,
            EmailService emailService,
            RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Conflict(new { message = "User with this email already exists." });

            if (request.UserType == UserType.Individual)
            {
                if (request.AddressId is null)
                    return BadRequest("Address is required.");

                var address = await _context.Addresses
    .Include(a => a.PostalCode)
        .ThenInclude(p => p.City)
            .ThenInclude(c => c.Country)
    .FirstOrDefaultAsync(a => a.Id == request.AddressId, ct);

                if (address is null)
                    return BadRequest("Address not found.");

                if (address.PostalCode == null ||
     address.PostalCode.City == null ||
     address.PostalCode.City.Country == null)
                {
                    return BadRequest("Address relations are corrupted.");
                }
            }

            var defaultRole = await EnsureRoleAsync("User", ct);
            if (defaultRole is null)
                return StatusCode(500, "Failed to resolve default role 'User'.");

            var user = new Login
            {
                UserName = request.Username,
                Email = request.Email,
                Name = request.Name,
                LastName = request.LastName,
                UserType = request.UserType,
                DateOfBirth = request.UserType == UserType.Individual ? request.DateOfBirth : null,

                AddressId = request.UserType == UserType.Individual ? request.AddressId : null,
                PhoneNumber = request.UserType == UserType.Individual ? request.PhoneNumber : request.CompanyPhone,

                CompanyName = request.UserType == UserType.LegalEntity ? request.CompanyName : null,
                TaxId = request.UserType == UserType.LegalEntity ? request.TaxId : null,
                RegistrationNumber = request.UserType == UserType.LegalEntity ? request.RegistrationNumber : null,
                ContactPerson = request.UserType == UserType.LegalEntity ? request.ContactPerson : null,
                ContactPosition = request.UserType == UserType.LegalEntity ? request.ContactPosition : null,
                CompanyPhone = request.UserType == UserType.LegalEntity ? request.CompanyPhone : null,
                ManualCity = request.UserType == UserType.LegalEntity ? request.ManualCity : null,
                ManualCountry = request.UserType == UserType.LegalEntity ? request.ManualCountry : null,
                ManualPostalCode = request.UserType == UserType.LegalEntity ? request.ManualPostalCode : null,
                ManualAddress = request.UserType == UserType.LegalEntity ? request.ManualAddress : null,

                AgreeToTerms = request.AgreeToTerms,
                AgreementVersion = request.AgreementVersion,
                IsEmailConfirmed = false,
                RoleId = defaultRole.Id
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            await _userManager.AddToRoleAsync(user, "User");

            try
            {
                await _emailService.SendEmailAsync(
                    request.Email,
                    "Welcome to Secure Logistics",
                    "You have successfully registered.",
                    null
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send welcome email.");
            }

            return Ok(new { message = "Registration successful.", userId = user.Id });
        }

        private async Task<Role?> EnsureRoleAsync(string roleName, CancellationToken ct)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
                return role;

            var newRole = new Role
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = "Default user role"
            };

            var create = await _roleManager.CreateAsync(newRole);
            return create.Succeeded ? newRole : null;
        }
    }
}

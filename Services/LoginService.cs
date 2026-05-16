using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.API;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Services
{
    public class LoginService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Login> _passwordHasher;

        public LoginService(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Login>();
        }

        // ============================================
        // GET USER BY ID
        // ============================================
        public async Task<Login?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // ============================================
        // GET USER BY EMAIL
        // ============================================
        public async Task<Login?> GetUserByEmailAsync(string email)
        {
            email = email.Trim().ToLower();

            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Email != null && u.Email.ToLower() == email);
        }

        // ============================================
        // UPDATE USER
        // ============================================
        public async Task UpdateUserAsync(Login user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // ============================================
        // VALIDATE LOGIN & PASSWORD
        // ============================================
        public async Task<(bool IsValid, Login? User)> ValidateUserAsync(string identifier, string password)
        {
            identifier = identifier.Trim().ToLower();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    (u.Email != null && u.Email.ToLower() == identifier) ||
                    (u.UserName != null && u.UserName.ToLower() == identifier)
                );

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return (false, null);

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                return (false, user);

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            );

            return (result == PasswordVerificationResult.Success, user);
        }

        // ============================================
        // REGISTER USER
        // ============================================
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterUserRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLower();
            var normalizedUsername = request.Username.Trim().ToLower();

            var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
                (u.Email != null && u.Email.ToLower() == normalizedEmail) ||
                (u.UserName != null && u.UserName.ToLower() == normalizedUsername));

            if (existingUser != null)
                return (false, "User already exists");

            var user = new Login
            {
                UserName = request.Username.Trim(),
                Email = normalizedEmail,
                Name = request.Name.Trim(),
                LastName = request.LastName.Trim(),
                DateOfBirth = request.DateOfBirth,

                PhoneNumber = request.UserType == UserType.Individual
        ? request.PhoneNumber
        : request.CompanyPhone,

                UserType = request.UserType,

                // Individual 3NF reference
                AddressId = request.AddressId,

                // Legal entity
                CompanyName = request.CompanyName,
                TaxId = request.TaxId,
                RegistrationNumber = request.RegistrationNumber,
                ContactPerson = request.ContactPerson,
                ContactPosition = request.ContactPosition,
                CompanyPhone = request.CompanyPhone,

                ManualCity = request.ManualCity,
                ManualCountry = request.ManualCountry,
                ManualPostalCode = request.ManualPostalCode,
                ManualAddress = request.ManualAddress,

                AgreeToTerms = request.AgreeToTerms,
                AgreementVersion = request.AgreementVersion,
                IsConfirmed = true,
                IsEmailConfirmed = false,

                UpdatedAt = DateTime.UtcNow
            };

            user.PasswordHash = HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "User registered successfully");
        }

        // ============================================
        // ACCESS FAILED (LOGIN / 2FA)
        // ============================================
        public async Task RegisterAccessFailedAsync(Login user)
        {
            user.AccessFailedCount++;
            user.LastLogin = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            if (user.AccessFailedCount >= 5)
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // ============================================
        // RESET FAILED COUNT
        // ============================================
        public async Task ResetAccessFailedAsync(Login user)
        {
            if (user.AccessFailedCount > 0 || user.LockoutEnd != null)
            {
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                user.LastLogin = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        // ============================================
        // PASSWORD HASH
        // ============================================
        public string HashPassword(Login user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        // ============================================
        // 2FA VERIFY
        // ============================================
        public bool VerifyTwoFactorToken(string secret, string code)
        {
            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(secret));
                return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
            }
            catch
            {
                return false;
            }
        }
    }
}
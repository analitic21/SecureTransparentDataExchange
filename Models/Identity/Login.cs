using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Feedback;
using SecureTransparentDataExchange.Models.Location;

namespace SecureTransparentDataExchange.Models.identity
{
    public class Login : IdentityUser<int>
    {
        // ============================================
        // Base identity fields
        // ============================================

        public override string? Email { get; set; }
        public override string? UserName { get; set; }
        public override string? PhoneNumber { get; set; }

        // ============================================
        // Personal data (nullable — admin safe)
        // ============================================

        public string? Name { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // ============================================
        // Address (nullable for admin + legal entity)
        // ============================================


        public int? AddressId { get; set; }
        

      

        [ForeignKey(nameof(AddressId))]
        public virtual Address? Address { get; set; }


        // ============================================
        // Manual address (legal entity)
        // ============================================

        public string? ManualCity { get; set; }
        public string? ManualCountry { get; set; }
        public string? ManualPostalCode { get; set; }
        public string? ManualAddress { get; set; }

        // ============================================
        // Legal entity fields
        // ============================================

        public string? RegistrationNumber { get; set; }
        public string? TaxId { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPosition { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyAddress { get; set; }

        // ============================================
        // User agreement
        // ============================================

        public bool AgreeToTerms { get; set; } = false;
        public string? AgreementVersion { get; set; }
        public bool IsConfirmed { get; set; } = false;
        public bool IsEmailConfirmed { get; set; } = false;

        // ============================================
        // Roles
        // ============================================

        public int? RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        public UserType? UserType { get; set; }

        // ============================================
        // 2FA
        // ============================================

        public string? TwoFactorSecretKey { get; set; }
        public bool IsTwoFactorEnabled { get; set; } = false;

        public string? RecoveryCodes { get; set; }
        public string? RecoveryCode { get; set; }
        public DateTime? RecoveryCodeExpiry { get; set; }

        // ============================================
        // Activity
        // ============================================

        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        // ============================================
        // Navigation collections (ОСТАВЛЕНЫ ВСЕ НУЖНЫЕ)
        // ============================================

        public virtual ICollection<BlockchainLog> BlockchainLogs { get; set; } = new List<BlockchainLog>();
        public virtual ICollection<LoginTransaction> LoginTransactions { get; set; } = new List<LoginTransaction>();
        public virtual ICollection<LoginAgreementConsent> LoginAgreementConsents { get; set; } = new List<LoginAgreementConsent>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
        public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<TwoFactorAuthModel> TwoFactorAuthModels { get; set; } = new List<TwoFactorAuthModel>();

      
        // ============================================
        // Constructors
        // ============================================

        public Login() { }

        public Login(string email, bool isEmailConfirmed = false)
        {
            Email = email;
            UserName = email;
            IsEmailConfirmed = isEmailConfirmed;
            PhoneNumber = string.Empty;
        }

        // ============================================
        // Password helpers disabled
        // ============================================

        public void SetPassword(string password) =>
            throw new NotSupportedException("Use LoginService for password hashing.");

        public bool VerifyPassword(string password) =>
            throw new NotSupportedException("Use LoginService for password verification.");
    }
}

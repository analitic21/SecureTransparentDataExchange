using SecureTransparentDataExchange.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Models
{
    public class LoginAgreementConsent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoginId { get; set; }

        [ForeignKey(nameof(LoginId))]
        public virtual Login Login { get; set; } = null!;

        [Required]
        public int AgreementId { get; set; }

        [ForeignKey(nameof(AgreementId))]
        public virtual LoginAgreement Agreement { get; set; } = null!;

        [Required]
        public bool ConsentGiven { get; set; }

        public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;

        // Example of additional check for uniqueness 
        public static bool IsConsentUnique(int loginId, int agreementId, ApplicationDbContext context)
        {
            return !context.LoginAgreementConsents
            .Any(c => c.LoginId == loginId && c.AgreementId == agreementId);
        }
    }
}
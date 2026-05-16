using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// ⚠ LEGACY MODEL
    /// This entity is NOT used for active Two-Factor Authentication.
    /// Kept only for database compatibility.
    /// 
    /// DO NOT store or read active 2FA secrets from this table.
    /// </summary>
    public class TwoFactorAuthModel
    {
        public int Id { get; set; }

        // ❌ NOT USED for active 2FA
        public string SecretKey { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ❌ NOT USED
        public ICollection<QRCodeModel> QRCodeModels { get; set; } = new List<QRCodeModel>();

        public int UserId { get; set; }
        public virtual Login? Login { get; set; }
    }
}

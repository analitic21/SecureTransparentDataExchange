using System;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        [Required]
        public string User { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public LogType? Type { get; set; }

        // Foreign Keys (both optional)
        public int? AppUserId { get; set; }
        public int? LoginId { get; set; }

        public virtual Login? Login { get; set; }
        public virtual AppUser? AppUser { get; set; }
    }
}

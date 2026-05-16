using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty; // The refresh token itself

        [Required]
        public DateTime ExpiryDate { get; set; } // The expiration date of the token

        [Required]
        public int UserId { get; set; } // The ID of the user for whom the token was issued

        [ForeignKey(nameof(UserId))]
        public virtual Login? User { get; set; } // Navigation property; made nullable to avoid constructor issues
    }
}

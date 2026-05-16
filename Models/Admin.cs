using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Models
{
    public class Admin
    {
       
        /// Unique identifier for the admin.
        [Key]
        public int AdminId { get; set; }

        
        /// Foreign key linking to the Login table.
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        
        /// Foreign key linking to the Role table.
        [Required]
        [ForeignKey("Role")]
        public int RoleId { get; set; }

        
        /// Date when the admin was created..
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// Last update date
        public DateTime? UpdatedAt { get; set; }

        
        /// Navigation property for the user.
        public virtual Login User { get; set; } = null!;

        /// Navigation property for the role.
        public virtual Role Role { get; set; } = null!;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }

        public Permission()
        {
            // Here you can initialize Role or other required fields 
            Role = new Role(); // Initialize an empty role if needed
        }
    }
}
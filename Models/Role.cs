using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class Role : IdentityRole<int>  // Ensure you're inheriting IdentityRole<int>
    {
        [Required]
        public string Description { get; set; } = "Default description";  // Add description if needed

        // Default constructor
        public Role() { }

        // Constructor with description parameter
        public Role(string description)
        {
            Description = description;
        }
    }
}

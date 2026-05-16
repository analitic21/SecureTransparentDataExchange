using SecureTransparentDataExchange.Models.Location;
using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        // Make it mandatory via required (if using C# 9 and above)
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        // If the phone is not mandatory, make it nullable
        public string? PhoneNumber { get; set; }

        // Similarly for FullName
        public string? FullName { get; set; }

        // Add other properties as needed
    }
}
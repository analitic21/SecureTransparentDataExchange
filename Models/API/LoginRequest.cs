using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models.API
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be between 8 and 100 characters.", MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }
}

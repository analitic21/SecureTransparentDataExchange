using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models.API
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Identifier is required (email or phone).")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A new password is required..")]
        [MinLength(8, ErrorMessage = "The password must be at least 8 characters long..")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

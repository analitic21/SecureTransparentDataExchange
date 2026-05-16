using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models.API
{
    public class RecoverPasswordModel
    {
        [Required(ErrorMessage = "Recovery code is required.")]
        public string RecoveryCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmation password is required.")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

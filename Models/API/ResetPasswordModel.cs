using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models.API
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Identifier (email or phone) is required.")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Recovery code is required.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "A new password is required..")]
        [MinLength(8, ErrorMessage = "The password must be at least 8 characters long..")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

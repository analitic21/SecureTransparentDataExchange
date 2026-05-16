using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.API
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Identifier is required (email or phone).")]
        public string Identifier { get; set; } = string.Empty;
    }
}

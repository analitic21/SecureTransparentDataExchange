using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.API
{
    public class AuthenticateRequest
    {
        public string? Email { get; set; }
        public string? Username { get; set; }

        [Required, StringLength(200)]
        public string Password { get; set; } = string.Empty;

        // 2FA token
        public string? Token { get; set; }
    }
}

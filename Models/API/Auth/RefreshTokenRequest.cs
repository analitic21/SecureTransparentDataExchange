using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.API.Auth
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}

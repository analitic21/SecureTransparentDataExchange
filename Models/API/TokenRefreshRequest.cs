using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models.API
{
    /// <summary>
    /// Request model for refreshing JWT token.
    /// </summary>
    public class TokenRefreshRequest
    {
        /// <summary>
        /// The refresh token received after authentication.
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

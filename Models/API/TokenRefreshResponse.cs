using System;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models.API
{
    /// <summary>
    /// Response model for refreshing a JWT token.
    /// </summary>
    public class TokenRefreshResponse
    {
        /// <summary>
        /// Indicates whether the token refresh operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Message providing details about the operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The new JWT access token.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// The new refresh token.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the response.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

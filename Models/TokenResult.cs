using System;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Represents the result of a token refresh operation.
    /// </summary>
    public class TokenResult
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The new JWT token, or null if not available.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// The new refresh token, or null if not available.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// A message describing the result of the operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Constructor for creating a TokenResult with all fields.
        /// </summary>
        public TokenResult(bool isSuccess, string message, string? token = null, string? refreshToken = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}

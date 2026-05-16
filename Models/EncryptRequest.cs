using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Request model for text encryption.
    /// </summary>
    public class EncryptRequest
    {
        private string _userId = string.Empty;
        private string _plainText = string.Empty;

        /// <summary>
        /// User ID for which encryption is performed (must be GUID format).
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [RegularExpression(@"^[a-fA-F0-9]{8}\-[a-fA-F0-9]{4}\-[a-fA-F0-9]{4}\-[a-fA-F0-9]{4}\-[a-fA-F0-9]{12}$",
            ErrorMessage = "Invalid UserId format. Expected GUID.")]
        public string UserId
        {
            get => _userId;
            set => _userId = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Plain text to encrypt.
        /// </summary>
        [Required(ErrorMessage = "PlainText is required.")]
        [StringLength(4096, ErrorMessage = "PlainText cannot exceed 4096 characters.")]
        public string PlainText
        {
            get => _plainText;
            set => _plainText = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Optional metadata for additional security verification.
        /// </summary>
        [StringLength(500, ErrorMessage = "Metadata cannot exceed 500 characters.")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Timestamp to prevent replay attacks.
        /// </summary>
        [Required(ErrorMessage = "Timestamp is required.")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Represents a request to decrypt text for a specific user.
    /// </summary>
    public class DecryptRequest
    {
        private string _userId = string.Empty;
        private string _encryptedText = string.Empty;

        /// <summary>
        /// Identifier of the user requesting the decryption (must be GUID format).
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
        /// The encrypted text that needs to be decrypted.
        /// </summary>
        [Required(ErrorMessage = "EncryptedText is required.")]
        [StringLength(4096, ErrorMessage = "EncryptedText cannot exceed 4096 characters.")]
        public string EncryptedText
        {
            get => _encryptedText;
            set => _encryptedText = value?.Trim() ?? string.Empty;
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

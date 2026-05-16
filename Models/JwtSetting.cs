using System;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Settings for JWT (JSON Web Token).
    /// </summary>
    public class JwtSetting
    {
        /// <summary>
        /// Unique identifier for the record in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key for linking with EncryptionKey.
        /// </summary>
        [Required]
        public int EncryptionKeyId { get; set; }

        /// <summary>
        /// Navigation property for linking with EncryptionKey.
        /// </summary>
        [Required]
        public EncryptionKey EncryptionKey { get; set; } = null!;

        /// <summary>
        /// JWT publisher.
        /// </summary>
        [Required]
        [StringLength(256, ErrorMessage = "Issuer length must not exceed 256 characters.")]
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// JWT audience.
        /// </summary>
        [Required]
        [StringLength(256, ErrorMessage = "Audience length must not exceed 256 characters.")]
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// JWT creation date.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The expiration date of the JWT.
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Determines whether the token has expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Determines whether the token is valid.
        /// </summary>
        public bool IsValid => DateTime.UtcNow >= CreatedAt && !IsExpired;

        /// <summary>
        /// The encrypted secret key.
        /// </summary>
        [Required]
        [StringLength(2048, ErrorMessage = "Encrypted secret key length must not exceed 2048 characters.")]
        public string EncryptedSecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new JWT with the specified expiration date.
        /// </summary>
        /// <param name="issuer">The issuer.</param>
        /// <param name="audience">The audience.</param>
        /// <param name="secretKey">The encrypted secret key.</param>
        /// <param name="expiresIn">The token expiration date (in minutes).</param>
        /// <returns>The new JWT setting.</returns>
        public static JwtSetting Create(string issuer, string audience, string secretKey, int expiresIn)
        {
            return new JwtSetting
            {
                Issuer = issuer,
                Audience = audience,
                EncryptedSecretKey = secretKey,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiresIn)
            };
        }
    }
}
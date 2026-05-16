using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models.API
{
    /// <summary>
    /// Model for user authentication (login).
    /// </summary>
    public class LoginModel : IValidatableObject
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "User type is required.")]
        public UserType UserType { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) User ID - typically used for internal tracking.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// IP address for anomaly detection and enhanced security.
        /// </summary>
        [Required(ErrorMessage = "IP Address is required.")]
        public string IPAddress { get; set; } = string.Empty;

        /// <summary>
        /// Two-Factor Authentication code for enhanced security.
        /// </summary>
        public string? TwoFactorCode { get; set; }

        /// <summary>
        /// Allows users to remain logged in longer without requiring re-authentication.
        /// </summary>
        public bool RememberMe { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate the format of the IP address
            if (!string.IsNullOrEmpty(IPAddress) && !System.Net.IPAddress.TryParse(IPAddress, out _))
            {
                yield return new ValidationResult("Invalid IP Address format.", new[] { nameof(IPAddress) });
            }

            // Validate TwoFactorCode only if provided
            if (!string.IsNullOrEmpty(TwoFactorCode) && TwoFactorCode.Length != 6)
            {
                yield return new ValidationResult("Invalid 2FA code format.", new[] { nameof(TwoFactorCode) });
            }

            // Additional validation logic can be added if necessary
            yield break; // No errors if all validations pass
        }
    }
}
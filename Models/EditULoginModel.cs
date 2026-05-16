using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Models.Enums; // Use a common Enum
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Represents a model for editing a user's login information.
    /// </summary>
    public class EditULoginModel
    {
        /// <summary>
        /// Identifier of the user to be edited.
        /// </summary>
        [Required(ErrorMessage = "User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int Id { get; set; } // User ID for editing

        /// <summary>
        /// Username of the user.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email of the user.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// First name of the user.
        /// </summary>
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Last name of the user.
        /// </summary>
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// New password for the user (optional).
        /// </summary>
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; } // Make password optional

        /// <summary>
        /// Password confirmation.
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; } // Make confirm password optional

        /// <summary>
        /// User type (individual or legal entity).
        /// </summary>
        [Required(ErrorMessage = "User type is required.")]
        public UserType UserType { get; set; } // Use a common Enum

        // Validation for optional password fields
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Password) && Password != ConfirmPassword)
            {
                yield return new ValidationResult("Passwords do not match.", new[] { nameof(ConfirmPassword) });
            }
        }
    }
}

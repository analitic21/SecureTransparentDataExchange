using SecureTransparentDataExchange.Models.Enums;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Models.API
{
    public class UpdateLoginModel
    {
        /// <summary>
        /// User ID (required for update).
        /// </summary>
        [Required(ErrorMessage = "User ID is required.")]
        public int Id { get; set; }

        /// <summary>
        /// Username.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User's last name.
        /// </summary>
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name must be less than 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Email.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100, ErrorMessage = "Email must be less than 100 characters.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User password (optional, only if the user wants to change it).
        /// </summary>
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; } // Now password is optional

        /// <summary>
        /// User type (Individual or Legal Entity).
        /// </summary>
        [Required(ErrorMessage = "User type is required.")]
        public UserType UserType { get; set; }

        /// <summary>
        /// User address (optional).
        /// </summary>
        [StringLength(200, ErrorMessage = "Address must be less than 200 characters.")]
        public string? Address { get; set; }

        /// <summary>
        /// Contact person (optional).
        /// </summary>
        [StringLength(100, ErrorMessage = "Contact person must be less than 100 characters.")]
        public string? ContactPerson { get; set; }
    }
}

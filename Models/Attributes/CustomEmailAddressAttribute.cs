using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SecureTransparentDataExchange.Models
{
    // Custom attribute for email validation 
    public class CustomEmailAddressAttribute : ValidationAttribute
    {
        public CustomEmailAddressAttribute() : base("Invalid email format.") { }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true; // If the value is null, consider it valid 
            }

            var email = value.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return false; // If the string is empty, return false
            }

            // Standard regular expression for checking email
            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailRegex); // Check if the value matches the regular expression
        }
    }

    public class UpdateLoginModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name must be less than 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [CustomEmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password should be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Models.API
{
    public class RegisterUserRequest : IValidatableObject
    {
        // Base fields
        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        // Individual — FK references
       
        public int? AddressId { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        // Legal entity
        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        public string? TaxId { get; set; }

        [StringLength(100)]
        public string? RegistrationNumber { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        public string? ContactPosition { get; set; }

        [Phone]
        public string? CompanyPhone { get; set; }

        // Manual address for legal entity
        [StringLength(100)]
        public string? ManualCity { get; set; }

        [StringLength(100)]
        public string? ManualCountry { get; set; }

        [StringLength(20)]
        public string? ManualPostalCode { get; set; }

        [StringLength(200)]
        public string? ManualAddress { get; set; }

        [Required]
        public bool AgreeToTerms { get; set; }

        [Required]
        public string? AgreementVersion { get; set; }

        [Required]
        public UserType UserType { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            //=======================================
            // INDIVIDUAL CHECKS (FK must exist)
            //=======================================
            if (UserType == UserType.Individual)
            {
               

                if (AddressId is null)
                    yield return new ValidationResult("Address is required for individuals.", new[] { nameof(AddressId) });

                if (DateOfBirth is null)
                    yield return new ValidationResult("Date of birth is required for individuals.", new[] { nameof(DateOfBirth) });

                if (string.IsNullOrWhiteSpace(PhoneNumber))
                    yield return new ValidationResult("Phone number is required for individuals.", new[] { nameof(PhoneNumber) });
            }

            //=======================================
            // LEGAL ENTITY VALIDATION
            //=======================================
            if (UserType == UserType.LegalEntity)
            {
                if (string.IsNullOrWhiteSpace(CompanyName))
                    yield return new ValidationResult("Company name is required.", new[] { nameof(CompanyName) });

                if (string.IsNullOrWhiteSpace(TaxId))
                    yield return new ValidationResult("Tax ID is required.", new[] { nameof(TaxId) });

                if (string.IsNullOrWhiteSpace(RegistrationNumber))
                    yield return new ValidationResult("Registration number is required.", new[] { nameof(RegistrationNumber) });

                if (string.IsNullOrWhiteSpace(ContactPerson))
                    yield return new ValidationResult("Contact person is required.", new[] { nameof(ContactPerson) });

                if (string.IsNullOrWhiteSpace(ContactPosition))
                    yield return new ValidationResult("Contact position is required.", new[] { nameof(ContactPosition) });

                if (string.IsNullOrWhiteSpace(CompanyPhone))
                    yield return new ValidationResult("Company phone is required.", new[] { nameof(CompanyPhone) });

                // manual address
                if (string.IsNullOrWhiteSpace(ManualCity))
                    yield return new ValidationResult("Manual city is required.", new[] { nameof(ManualCity) });

                if (string.IsNullOrWhiteSpace(ManualCountry))
                    yield return new ValidationResult("Manual country is required.", new[] { nameof(ManualCountry) });

                if (string.IsNullOrWhiteSpace(ManualPostalCode))
                    yield return new ValidationResult("Manual postal code is required.", new[] { nameof(ManualPostalCode) });

                if (string.IsNullOrWhiteSpace(ManualAddress))
                    yield return new ValidationResult("Manual address is required.", new[] { nameof(ManualAddress) });
            }
        }
    }
}

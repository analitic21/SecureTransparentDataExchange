using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Models
{
    public class RegisterModel : IValidatableObject
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }   // ← было Phone

        [Required]
        public UserType UserType { get; set; }

        [Required]
        public bool AgreeToTerms { get; set; }

        [Required]
        public string AgreementVersion { get; set; } = string.Empty;

        // --- For Individual (linked references) ---
        public int? CityId { get; set; }
        public int? CountryId { get; set; }       // ← added
        public int? PostalCodeId { get; set; }   // ← added
        public int? AddressId { get; set; }      // ← added

        // --- For Legal Entity (manual address) ---
        public string? RegistrationNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxId { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPosition { get; set; }
        public string? CompanyPhone { get; set; }

        public string? ManualCity { get; set; }
        public string? ManualCountry { get; set; }
        public string? ManualPostalCode { get; set; }
        public string? ManualAddress { get; set; }

        public bool IsConfirmed { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Normalize strings (null safe)
            Username = Username?.Trim() ?? "";
            Email = Email?.Trim() ?? "";
            Name = Name?.Trim() ?? "";
            LastName = LastName?.Trim() ?? "";
            AgreementVersion = AgreementVersion?.Trim() ?? "";
            PhoneNumber = PhoneNumber?.Trim();
            RegistrationNumber = RegistrationNumber?.Trim();
            CompanyName = CompanyName?.Trim();
            TaxId = TaxId?.Trim();
            ContactPerson = ContactPerson?.Trim();
            ContactPosition = ContactPosition?.Trim();
            CompanyPhone = CompanyPhone?.Trim();
            ManualCity = ManualCity?.Trim();
            ManualCountry = ManualCountry?.Trim();
            ManualPostalCode = ManualPostalCode?.Trim();
            ManualAddress = ManualAddress?.Trim();

            if (UserType == UserType.Individual)
            {
                if (!DateOfBirth.HasValue)
                    yield return new ValidationResult("Date of birth is required.", new[] { nameof(DateOfBirth) });

                if (AddressId is null)
                    yield return new ValidationResult("AddressId is required.", new[] { nameof(AddressId) });

                if (string.IsNullOrWhiteSpace(PhoneNumber))
                    yield return new ValidationResult("PhoneNumber is required.", new[] { nameof(PhoneNumber) });
            }

            if (UserType == UserType.LegalEntity)
            {
                if (string.IsNullOrWhiteSpace(RegistrationNumber))
                    yield return new ValidationResult("Registration Number is required.", new[] { nameof(RegistrationNumber) });

                if (string.IsNullOrWhiteSpace(CompanyName))
                    yield return new ValidationResult("Company Name is required.", new[] { nameof(CompanyName) });

                if (string.IsNullOrWhiteSpace(TaxId))
                    yield return new ValidationResult("Tax ID is required.", new[] { nameof(TaxId) });

                if (string.IsNullOrWhiteSpace(ContactPerson))
                    yield return new ValidationResult("Contact Person is required.", new[] { nameof(ContactPerson) });

                if (string.IsNullOrWhiteSpace(ContactPosition))
                    yield return new ValidationResult("Contact Position is required.", new[] { nameof(ContactPosition) });

                if (string.IsNullOrWhiteSpace(CompanyPhone))
                    yield return new ValidationResult("Company Phone is required.", new[] { nameof(CompanyPhone) });

                if (string.IsNullOrWhiteSpace(ManualCity))
                    yield return new ValidationResult("Manual City is required.", new[] { nameof(ManualCity) });

                if (string.IsNullOrWhiteSpace(ManualCountry))
                    yield return new ValidationResult("Manual Country is required.", new[] { nameof(ManualCountry) });

                if (string.IsNullOrWhiteSpace(ManualPostalCode))
                    yield return new ValidationResult("Manual Postal Code is required.", new[] { nameof(ManualPostalCode) });

                if (string.IsNullOrWhiteSpace(ManualAddress))
                    yield return new ValidationResult("Manual Address is required.", new[] { nameof(ManualAddress) });
            }
        }
    }
}

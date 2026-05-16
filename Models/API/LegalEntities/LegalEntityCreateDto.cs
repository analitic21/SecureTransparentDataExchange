using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.API.LegalEntities
{
    public record LegalEntityCreateDto(
        [Required, StringLength(200)] string CompanyName,
        [StringLength(50)] string? TaxId,
        [StringLength(100)] string? RegistrationNumber,
        [StringLength(100)] string? ContactPerson,
        [StringLength(100)] string? ContactPosition,
        string? CompanyPhone, // ❗ убрал [Phone]
        [Required, StringLength(100)] string ManualCountry,
        [Required, StringLength(100)] string ManualCity,
        [Required, StringLength(20)] string ManualPostalCode,
        [Required, StringLength(200)] string ManualAddress
    );
}
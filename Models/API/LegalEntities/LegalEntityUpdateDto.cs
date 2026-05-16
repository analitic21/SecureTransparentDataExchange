using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.API.LegalEntities
{
    public record LegalEntityUpdateDto(
        [Required] int Id,
        [Required, StringLength(200)] string CompanyName,
        [StringLength(50)] string? TaxId,
        [StringLength(100)] string? RegistrationNumber,
        [StringLength(100)] string? ContactPerson,
        [StringLength(100)] string? ContactPosition,
        [Phone] string? CompanyPhone,
        [StringLength(100)] string? ManualCountry,
        [StringLength(100)] string? ManualCity,
        [StringLength(20)] string? ManualPostalCode,
        [StringLength(200)] string? ManualAddress
    );
}

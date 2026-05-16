using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Location;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureTransparentDataExchange.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string ContactNumber { get; set; } = string.Empty;

        public CompanyType CompanyType { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        public int LegalEntityId { get; set; }

        [ForeignKey(nameof(LegalEntityId))]
        public LegalEntity LegalEntity { get; set; } = null!;

        public int? AddressId { get; set; }

        [ForeignKey(nameof(AddressId))]
        public Address? CompanyAddress { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        public string? ContactInfo { get; set; }

        [Required]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        public string ContactPerson { get; set; } = string.Empty;
    }
}
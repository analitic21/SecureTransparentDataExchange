using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Orders;

namespace SecureTransparentDataExchange.Models
{
    public class LegalEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoginId { get; set; } // FK -> Login 

        [ForeignKey(nameof(LoginId))]
        public Login Login { get; set; } = null!;

        [Required, StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        // Often unique. If it’s not always there, make it nullable. 
        [StringLength(100)]
        public string? RegistrationNumber { get; set; }

        [StringLength(50)]
        public string? TaxId { get; set; }

        // Contacts may not arrive immediately - we'll make them optional.
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        public Company? Company { get; set; }
        [StringLength(100)]
        public string? ContactPosition { get; set; }

        // No [Phone], as it's strict. Just a string.
        [StringLength(50)]
        public string? CompanyPhone { get; set; }

        // "Manual address" is often optional. If you want to require it, leave it as Required.
        [StringLength(100)]
        public string? ManualCity { get; set; }

        [StringLength(100)]
        public string? ManualCountry { get; set; }

        [StringLength(20)]
        public string? ManualPostalCode { get; set; }

        [StringLength(200)]
        public string? ManualAddress { get; set; }

        // Technical fields (optional) 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
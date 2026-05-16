using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.DTOs
{
    public class CountryCreateDTO
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;
    }
}

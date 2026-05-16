using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.DTOs
{
    public class PostalCodeDTO
    {
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;
    }
}

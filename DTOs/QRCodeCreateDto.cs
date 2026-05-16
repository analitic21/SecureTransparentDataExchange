using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.DTOs
{
    public class QRCodeCreateDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // опционально — если хочешь передавать готовый secret
        public string? SecretKey { get; set; }
    }
}

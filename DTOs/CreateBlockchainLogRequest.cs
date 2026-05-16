using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.DTOs
{
    public class CreateBlockchainLogRequest
    {
        [Required]
        public int ShipmentId { get; set; }

        [Required]
        public string Data { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
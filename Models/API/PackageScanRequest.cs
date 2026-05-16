using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.API
{
    public class PackageScanRequest
    {
        [Required]
        public string TrackingNumber { get; set; } = string.Empty;
    }
}

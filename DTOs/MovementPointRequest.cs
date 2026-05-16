using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.DTOs.Realtime
{
    public class MovementPointRequest
    {
        [Required]
        public int DeviceId { get; set; }

        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lng { get; set; }
    }
}
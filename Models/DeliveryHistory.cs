using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureTransparentDataExchange.Models
{
    public class DeliveryHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DeliveryRouteId { get; set; }

        [ForeignKey(nameof(DeliveryRouteId))]
        public virtual DeliveryRoute DeliveryRoute { get; set; } = null!;

        [Required]
        public decimal ActualCost { get; set; }

        [Required]
        public double ActualDistanceInKm { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveredAt { get; set; }

        public string? Remarks { get; set; }

        // Link to the client who arranged the delivery 
        [Required]
        public int ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public virtual Client Client { get; set; } = null!;
    }
}
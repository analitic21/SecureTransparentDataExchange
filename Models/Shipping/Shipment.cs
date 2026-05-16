using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Billing;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureTransparentDataExchange.Models.Shipping
{
    public class Shipment
    {
        [Key]
        public int Id { get; set; }

       
        public string TrackingNumber { get; set; } = string.Empty;

        [Required]
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // OWNER (Client)
        public int? ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public Client? Client { get; set; }
        // ORDER (optional)
        public int? OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
        public int? DeliveryRouteId { get; set; }

        [ForeignKey(nameof(DeliveryRouteId))]
        public DeliveryRoute? DeliveryRoute { get; set; }
        // RELATIONS
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<CardTransaction> CardTransactions { get; set; } = new List<CardTransaction>();
        public ICollection<TrackingNumberEntity> TrackingNumbers { get; set; } = new List<TrackingNumberEntity>();
        public ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();
        public ICollection<Package> Packages { get; set; } = new List<Package>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

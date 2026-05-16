using System;

namespace SecureTransparentDataExchange.Models.Enums
{
    public enum ShipmentStatus
    {
        Created,
        Processing,
        ReadyForPickup,
        InTransit,
        OutForDelivery,
        Delivered,
        Delayed,
        Pending,
        PaymentCompleted,
        Returned,
        Canceled,
        Generated
    }
}

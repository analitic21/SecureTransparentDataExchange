using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Shipping;

namespace SecureTransparentDataExchange.Services
{
    /// <summary>
    /// Centralized shipment status transitions manager.
    /// </summary>
    public class ShipmentStatusManager
    {
        public void SetCreated(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.Created;
        }

        public void SetProcessing(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.Processing;
        }

        public void SetReadyForPickup(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.ReadyForPickup;
        }

        public void SetInTransit(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.InTransit;
        }

        public void SetOutForDelivery(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.OutForDelivery;
        }

        public void SetDelivered(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.Delivered;
        }

        public void SetCancelled(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.Canceled;
        }

        public void SetReturned(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.Returned;
        }

        public void SetPaymentCompleted(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.PaymentCompleted;
        }

        public void SetDelayed(Shipment shipment)
        {
            shipment.Status = ShipmentStatus.Delayed;
        }
    }
}

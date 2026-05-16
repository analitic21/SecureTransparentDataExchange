using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Models
{
    public class TrackingNumberModel
    {
        public string TrackingNumber { get; set; } // String type for tracking number
        public ShipmentStatus ShipmentStatus { get; set; } // Shipment status

        // Constructor with default values
        public TrackingNumberModel(string trackingNumber = "0", ShipmentStatus shipmentStatus = ShipmentStatus.Created)
        {
            TrackingNumber = trackingNumber; // Type changed to string
            ShipmentStatus = shipmentStatus; // Set the default status
        }

        // Method for creating TrackingNumberModel with default values
        public static TrackingNumberModel Created()
        {
            return new TrackingNumberModel(); // Create an object with default values
        }

        // Method for creating TrackingNumberModel with a given ShipmentStatus
        public static TrackingNumberModel WithStatus(ShipmentStatus shipmentStatus)
        {
            return new TrackingNumberModel(shipmentStatus: shipmentStatus); // Use a static method with the ShipmentStatus parameter
        }
    }
}
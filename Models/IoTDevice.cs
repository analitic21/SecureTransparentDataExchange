using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Models
{
    public class IoTDevice
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Communication with the package 
        public int? ParcelId { get; set; }
        public virtual Parcel? Parcel { get; set; }
    }
}
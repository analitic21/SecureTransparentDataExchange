using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Services.Realtime
{
    public class DeliveryDestinationResolver
    {
        private readonly ApplicationDbContext _context;

        public DeliveryDestinationResolver(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(double lat, double lng)?> GetDestinationForDeviceAsync(int deviceId)
        {
            var destination = await _context.IoTDevices
                .Where(d => d.Id == deviceId)
                .Select(d => d.Parcel == null ||
                             d.Parcel.Shipment == null ||
                             d.Parcel.Shipment.DeliveryRoute == null
                    ? null
                    : new
                    {
                        Lat = d.Parcel.Shipment.DeliveryRoute.DestinationLatitude,
                        Lng = d.Parcel.Shipment.DeliveryRoute.DestinationLongitude
                    })
                .FirstOrDefaultAsync();

            if (destination?.Lat == null || destination.Lng == null)
                return null;

            return (destination.Lat.Value, destination.Lng.Value);
        }
    }
}
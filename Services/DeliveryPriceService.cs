using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Services
{
    public class DeliveryPriceService
    {
        private readonly ApplicationDbContext _context;

        private const decimal PricePerKm = 0.5m;
        private const decimal BasePrice = 5.00m;

        public DeliveryPriceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DeliveryPriceResponse> CalculateDeliveryPriceAsync(
            int originAddressId,
            int destinationAddressId)
        {
            var origin = await _context.Addresses.FindAsync(originAddressId);
            var destination = await _context.Addresses.FindAsync(destinationAddressId);

            if (origin == null || destination == null)
            {
                return new DeliveryPriceResponse
                {
                    Success = false,
                    Message = "Origin or destination address not found."
                };
            }

            if (!origin.Latitude.HasValue || !origin.Longitude.HasValue ||
                !destination.Latitude.HasValue || !destination.Longitude.HasValue)
            {
                return new DeliveryPriceResponse
                {
                    Success = false,
                    Message = "Address coordinates are not set."
                };
            }

            var distance = CalculateDistanceInKm(
                origin.Latitude.Value,
                origin.Longitude.Value,
                destination.Latitude.Value,
                destination.Longitude.Value
            );

            var distancePrice = (decimal)distance * PricePerKm;
            var totalPrice = BasePrice + distancePrice;

            return new DeliveryPriceResponse
            {
                Success = true,
                Message = "Delivery price calculated successfully.",
                DistanceInKm = Math.Round(distance, 2),
                Price = Math.Round(totalPrice, 2)
            };
        }

        private static double CalculateDistanceInKm(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            const double earthRadiusKm = 6371;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using  SecureTransparentDataExchange.DTOs;

namespace SecureTransparentDataExchange.Services
{
    public class SmartDeliveryService
    {
        private readonly ApplicationDbContext _db;
        private readonly RouteService _routeService;

        private const decimal PricePerKg = 2.50m;
        private const decimal PricePerKm = 0.60m;

        public SmartDeliveryService(ApplicationDbContext db, RouteService routeService)
        {
            _db = db;
            _routeService = routeService;
        }

        public async Task<object> ProcessAsync(SmartDeliveryRequest request)
        {
            var parcel = await _db.Parcels
                .FirstOrDefaultAsync(p => p.Id == request.ParcelId);

            if (parcel == null)
                throw new InvalidOperationException("Parcel not found.");

            if (request.Distances == null || request.Distances.Count == 0)
                throw new InvalidOperationException("Distance matrix is required.");

            int size = request.Distances.Count;
            int[,] matrix = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                if (request.Distances[i].Count != size)
                    throw new InvalidOperationException("Distance matrix must be square.");

                for (int j = 0; j < size; j++)
                    matrix[i, j] = request.Distances[i][j];
            }

            var optimized = _routeService.GetOptimizedRoute(matrix);

            decimal distanceCost = (decimal)optimized.TotalDistance * PricePerKm;
            decimal weightCost = parcel.WeightKg * PricePerKg;
            decimal packageCost = parcel.PackageCost;
            decimal totalCost = distanceCost + weightCost + packageCost;

            return new
            {
                parcelId = parcel.Id,
                trackingNumber = parcel.TrackingNumber,
                optimizedRoute = optimized.Route,
                totalDistance = optimized.TotalDistance,
                weightKg = parcel.WeightKg,
                cargoValue = parcel.CargoValue,
                packageCost,
                distanceCost,
                weightCost,
                totalCost
            };
        }
    }
}
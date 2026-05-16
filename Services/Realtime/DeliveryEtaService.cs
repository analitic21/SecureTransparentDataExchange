using System;

namespace SecureTransparentDataExchange.Services.Realtime
{
    public class DeliveryEtaService
    {
        private const double EarthRadiusKm = 6371.0;

        public DeliveryEtaResult CalculateEta(
            double currentLat,
            double currentLng,
            double destinationLat,
            double destinationLng,
            double speedKmh)
        {
            var distanceKm = CalculateDistanceKm(
                currentLat,
                currentLng,
                destinationLat,
                destinationLng
            );

            if (speedKmh <= 0)
            {
                return new DeliveryEtaResult
                {
                    DistanceKm = distanceKm,
                    SpeedKmh = speedKmh,
                    EstimatedArrivalUtc = null,
                    RemainingMinutes = null,
                    Message = "ETA unavailable (speed is zero)"
                };
            }

            var hours = distanceKm / speedKmh;
            var minutes = hours * 60;
            var eta = DateTime.UtcNow.AddHours(hours);

            return new DeliveryEtaResult
            {
                DistanceKm = Math.Round(distanceKm, 2),
                SpeedKmh = Math.Round(speedKmh, 2),
                EstimatedArrivalUtc = eta,
                RemainingMinutes = Math.Round(minutes, 1),
                Message = "ETA calculated"
            };
        }

        public bool IsArrived(
            double currentLat,
            double currentLng,
            double destinationLat,
            double destinationLng,
            double thresholdMeters = 100)
        {
            var distanceKm = CalculateDistanceKm(
                currentLat,
                currentLng,
                destinationLat,
                destinationLng
            );

            return distanceKm * 1000 <= thresholdMeters;
        }

        private static double CalculateDistanceKm(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private static double DegreesToRadians(double deg)
        {
            return deg * Math.PI / 180.0;
        }
    }

    public class DeliveryEtaResult
    {
        public double DistanceKm { get; set; }
        public double SpeedKmh { get; set; }
        public DateTime? EstimatedArrivalUtc { get; set; }
        public double? RemainingMinutes { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
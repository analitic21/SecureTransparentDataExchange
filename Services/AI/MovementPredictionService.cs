using System.Collections.Concurrent;

namespace SecureTransparentDataExchange.Services.AI
{
    public class MovementPredictionService
    {
        // Store history in memory (without changing the database)
        private readonly ConcurrentDictionary<int, List<TelemetryPoint>> _history = new();
        private const int MaxPoints = 200;// limit memory

        public void AddPoint(int deviceId, double lat, double lng, DateTime utc)
        {
            var list = _history.GetOrAdd(deviceId, _ => new List<TelemetryPoint>());

            lock (list)
            {
                list.Add(new TelemetryPoint(lat, lng, utc));

                if (list.Count > MaxPoints)
                    list.RemoveRange(0, list.Count - MaxPoints);
            }
        }

        public TelemetryPoint? PredictNext(int deviceId)
        {
            if (!_history.TryGetValue(deviceId, out var list))
                return null;

            lock (list)
            {
                if (list.Count < 2)
                    return null;

                var last = list[^1];
                var prev = list[^2];

                // простой прогноз: last + (last-prev)
                var dLat = last.Latitude - prev.Latitude;
                var dLng = last.Longitude - prev.Longitude;

                return new TelemetryPoint(
                    last.Latitude + dLat,
                    last.Longitude + dLng,
                    DateTime.UtcNow
                );
            }
        }

        public TelemetryPoint? PredictAfterSeconds(int deviceId, int secondsAhead)
        {
            if (!_history.TryGetValue(deviceId, out var list))
                return null;

            lock (list)
            {
                if (list.Count < 2)
                    return null;

                var last = list[^1];
                var prev = list[^2];

                var dt = (last.Utc - prev.Utc).TotalSeconds;
                if (dt <= 0)
                    return null;

                // Rate of change of coordinates "per second" (very simple estimate)
                var vLat = (last.Latitude - prev.Latitude) / dt;
                var vLng = (last.Longitude - prev.Longitude) / dt;

                var predictedLat = last.Latitude + vLat * secondsAhead;
                var predictedLng = last.Longitude + vLng * secondsAhead;

                return new TelemetryPoint(predictedLat, predictedLng, DateTime.UtcNow.AddSeconds(secondsAhead));
            }
        }

        public MovementStats? GetStats(int deviceId)
        {
            if (!_history.TryGetValue(deviceId, out var list))
                return null;

            lock (list)
            {
                if (list.Count < 2)
                    return null;

                var last = list[^1];
                var prev = list[^2];

                var distanceKm = HaversineKm(prev.Latitude, prev.Longitude, last.Latitude, last.Longitude);
                var hours = (last.Utc - prev.Utc).TotalHours;

                var speedKmh = hours <= 0 ? 0 : distanceKm / hours;

                return new MovementStats
                {
                    LastLat = last.Latitude,
                    LastLng = last.Longitude,
                    SpeedKmh = speedKmh,
                    DistanceKmLastStep = distanceKm,
                    LastUpdateUtc = last.Utc
                };
            }
        }

        public IReadOnlyList<TelemetryPoint> GetHistory(int deviceId)
        {
            if (!_history.TryGetValue(deviceId, out var list))
                return Array.Empty<TelemetryPoint>();

            lock (list)
            {
                return list.ToList();
            }
        }

        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double deg) => deg * (Math.PI / 180.0);

        public record TelemetryPoint(double Latitude, double Longitude, DateTime Utc);

        public class MovementStats
        {
            public double LastLat { get; set; }
            public double LastLng { get; set; }
            public double SpeedKmh { get; set; }
            public double DistanceKmLastStep { get; set; }
            public DateTime LastUpdateUtc { get; set; }
        }
    }
}

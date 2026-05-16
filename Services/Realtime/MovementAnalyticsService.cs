using System.Collections.Concurrent;

namespace SecureTransparentDataExchange.Services.Realtime
{
    public class MovementAnalyticsService
    {
        private readonly ConcurrentDictionary<int, List<(double lat, double lng, DateTime t)>> _history
            = new();

        public void AddPoint(int deviceId, double lat, double lng)
        {
            var list = _history.GetOrAdd(deviceId, _ => new List<(double, double, DateTime)>());
            list.Add((lat, lng, DateTime.UtcNow));

            if (list.Count > 500)
                list.RemoveAt(0);
        }
        public double CalculateSpeed(int deviceId)
        {
            if (!_history.TryGetValue(deviceId, out var list) || list.Count < 2)
                return 0;

            var last = list[^1];
            var prev = list[^2];

            var dist = GeoDistance(prev.lat, prev.lng, last.lat, last.lng);
            var time = (last.t - prev.t).TotalHours;

            return time == 0 ? 0 : dist / time;
        }

        private double GeoDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = Math.PI * (lat2 - lat1) / 180;
            var dLon = Math.PI * (lon2 - lon1) / 180;

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        public IEnumerable<(double lat, double lng, DateTime t)> GetHistory(int deviceId)
        {
            return _history.TryGetValue(deviceId, out var list)
                ? list
                : Enumerable.Empty<(double, double, DateTime)>();
        }
    }
}

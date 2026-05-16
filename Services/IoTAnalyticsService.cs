public class IoTAnalyticsService
{
    public double CalculateDistance(List<IoTMovementDto> points)
    {
        double total = 0;
        for (int i = 1; i < points.Count; i++)
        {
            total += Haversine(points[i - 1], points[i]);
        }
        return total;
    }

    private double Haversine(IoTMovementDto p1, IoTMovementDto p2)
    {
        const double R = 6371;
        var dLat = ToRad(p2.Latitude - p1.Latitude);
        var dLon = ToRad(p2.Longitude - p1.Longitude);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRad(p1.Latitude)) *
            Math.Cos(ToRad(p2.Latitude)) *
            Math.Sin(dLon / 2) *
            Math.Sin(dLon / 2);

        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private double ToRad(double v) => v * Math.PI / 180;
}

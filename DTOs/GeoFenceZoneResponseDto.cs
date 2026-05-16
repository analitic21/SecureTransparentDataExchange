namespace SecureTransparentDataExchange.DTOs
{
    public class GeoFenceZoneResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public int RadiusMeters { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Services
{
    public class GeoFenceSecurityService
    {
        private readonly ApplicationDbContext _db;

        public GeoFenceSecurityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GeoFenceZone> CreateZoneWithKeyAsync(
            string name,
            double centerLat,
            double centerLng,
            int radiusMeters)
        {
            var key = new EncryptionKey
            {
                PublicKey = Guid.NewGuid().ToString("N"),
                PrivateKey = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1)
            };

            _db.EncryptionKeys.Add(key);
            await _db.SaveChangesAsync();

            var zone = new GeoFenceZone
            {
                Name = name,
                CenterLat = centerLat,
                CenterLng = centerLng,
                RadiusMeters = radiusMeters,
                EncryptionKeyId = key.Id,
                CreatedAt = DateTime.UtcNow
            };

            _db.GeoFenceZones.Add(zone);
            await _db.SaveChangesAsync();

            return zone;
        }
    }
}
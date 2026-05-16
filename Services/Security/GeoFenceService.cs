using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services.Security
{
    public class GeoFenceService
    {
        private readonly ApplicationDbContext _context;

        public GeoFenceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GeoFenceZone>> GetAllAsync()
        {
            return await _context.GeoFenceZones
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<GeoFenceZone> CreateAsync(GeoFenceZone zone)
        {
            _context.GeoFenceZones.Add(zone);
            await _context.SaveChangesAsync();
            return zone;
        }

        public async Task DeleteAsync(int id)
        {
            var zone = await _context.GeoFenceZones.FindAsync(id);
            if (zone != null)
            {
                _context.GeoFenceZones.Remove(zone);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<GeoFenceZone?> UpdateAsync(int id, GeoFenceZone updated)
        {
            var zone = await _context.GeoFenceZones.FindAsync(id);

            if (zone == null)
                return null;

            zone.Name = updated.Name;
            zone.CenterLat = updated.CenterLat;
            zone.CenterLng = updated.CenterLng;
            zone.RadiusMeters = updated.RadiusMeters;
            zone.EncryptionKeyId = updated.EncryptionKeyId;

            await _context.SaveChangesAsync();

            return zone;
        }
        public bool IsInsideZone(GeoFenceZone zone, double lat, double lng)
        {
            var distance = DistanceMeters(
                zone.CenterLat,
                zone.CenterLng,
                lat,
                lng
            );

            return distance <= zone.RadiusMeters;
        }
        public async Task<GeoFenceZone?> GetByIdAsync(int id)
        {
            return await _context.GeoFenceZones
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        private static double DistanceMeters(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371000.0;

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static double DegreesToRadians(double deg)
            => deg * (Math.PI / 180.0);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;

namespace SecureTransparentDataExchange.Services
{
    public class DeliveryRouteService
    {
        private readonly ApplicationDbContext _context;

        public DeliveryRouteService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<DeliveryRoute>> GetAllRoutesAsync()
        {
            return await _context.DeliveryRoutes
                .Include(r => r.OriginCountry)
                .Include(r => r.OriginCity)
                .Include(r => r.OriginPostalCode)
                .Include(r => r.DestinationCountry)
                .Include(r => r.DestinationCity)
                .Include(r => r.DestinationPostalCode)
                .ToListAsync();
        }

        public async Task<DeliveryRoute?> GetRouteByIdAsync(int id)
        {
            return await _context.DeliveryRoutes
                .Include(r => r.OriginCountry)
                .Include(r => r.OriginCity)
                .Include(r => r.OriginPostalCode)
                .Include(r => r.DestinationCountry)
                .Include(r => r.DestinationCity)
                .Include(r => r.DestinationPostalCode)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<DeliveryRoute> CreateRouteAsync(DeliveryRoute route)
        {
            _context.DeliveryRoutes.Add(route);
            await _context.SaveChangesAsync();
            return route;
        }
        public async Task<bool> DeleteRouteAsync(int id)
        {
            var route = await _context.DeliveryRoutes.FindAsync(id);

            if (route == null)
                return false;

            _context.DeliveryRoutes.Remove(route);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<DeliveryRoute?> UpdateRouteAsync(int id, DeliveryRoute updatedRoute)
        {
            if (updatedRoute == null)
                throw new ArgumentNullException(nameof(updatedRoute));

            var existingRoute = await _context.DeliveryRoutes.FindAsync(id);

            if (existingRoute == null)
                return null;

            existingRoute.OriginCountryId = updatedRoute.OriginCountryId;
            existingRoute.OriginCityId = updatedRoute.OriginCityId;
            existingRoute.OriginPostalCodeId = updatedRoute.OriginPostalCodeId;
            existingRoute.DestinationCountryId = updatedRoute.DestinationCountryId;
            existingRoute.DestinationCityId = updatedRoute.DestinationCityId;
            existingRoute.DestinationPostalCodeId = updatedRoute.DestinationPostalCodeId;

            await _context.SaveChangesAsync();

            return existingRoute;
        }
    }
}

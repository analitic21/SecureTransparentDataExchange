using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;

namespace SecureTransparentDataExchange.Services
{
    public class PackageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PackageService> _logger;

        public PackageService(ApplicationDbContext context, ILogger<PackageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Package> RegisterOrUpdatePackageAsync(string trackingNumber)
        {
            var existing = await _context.Packages.FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber);

            if (existing != null)
            {
                existing.Status = "Scanned";
                existing.UpdatedAt = DateTime.UtcNow;
                _context.Packages.Update(existing);
                _logger.LogInformation("Package updated: {TrackingNumber}", trackingNumber);
                await _context.SaveChangesAsync();
                return existing;
            }

            var newPackage = new Package
            {
                TrackingNumber = trackingNumber,
                Status = "Registered",
                CreatedAt = DateTime.UtcNow
            };

            await _context.Packages.AddAsync(newPackage);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New package registered: {TrackingNumber}", trackingNumber);

            return newPackage;
        }
    }
}

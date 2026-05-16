using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Repositories
{
    public class TrackingNumberRepository
    {
        private readonly ApplicationDbContext _context;

        public TrackingNumberRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Check for null for the context
        }

        // Method for saving TrackingNumber to the database (asynchronously)
        public async Task AddAsync(TrackingNumberEntity trackingNumber)
        {
            if (trackingNumber == null)
            {
                throw new ArgumentNullException(nameof(trackingNumber)); // Check for null
            }

            await _context.TrackingNumbers.AddAsync(trackingNumber); // Add TrackingNumber to the database
            await _context.SaveChangesAsync(); // Save changes to the database
        }

        // Get tracking number by ID
        public async Task<TrackingNumberEntity?> GetTrackingNumberByIdAsync(int id)
        {
            // Return null if TrackingNumber not found
            return await _context.TrackingNumbers.FindAsync(id);
        }

        // Get all tracking numbers
        public async Task<List<TrackingNumberEntity>> GetAllTrackingNumbersAsync()
        {
            return await _context.TrackingNumbers.ToListAsync();
        }

        // Update tracking number
        public async Task UpdateTrackingNumberAsync(TrackingNumberEntity trackingNumber)
        {
            if (trackingNumber == null)
            {
                throw new ArgumentNullException(nameof(trackingNumber)); // Check for null
            }

            _context.TrackingNumbers.Update(trackingNumber);
            await _context.SaveChangesAsync();
        }

        // Delete tracking number by ID
        public async Task DeleteTrackingNumberAsync(int id)
        {
            var trackingNumber = await GetTrackingNumberByIdAsync(id);
            if (trackingNumber != null)
            {
                _context.TrackingNumbers.Remove(trackingNumber);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Log that the object is not found
                throw new KeyNotFoundException($"Tracking number with id {id} not found.");
            }
        }
    }
}
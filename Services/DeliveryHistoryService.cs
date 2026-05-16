using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SecureTransparentDataExchange.Services
{
    public class DeliveryHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeliveryHistoryService> _logger;

        public DeliveryHistoryService(
            ApplicationDbContext context,
            ILogger<DeliveryHistoryService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<DeliveryHistory>> GetAllDeliveryHistoriesAsync()
        {
            return await _context.DeliveryHistories
                .AsNoTracking()
                .Include(dh => dh.DeliveryRoute)
                .Include(dh => dh.Client)
                .ToListAsync();
        }

        public async Task<DeliveryHistory> CreateDeliveryHistoryAsync(DeliveryHistory history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            _context.DeliveryHistories.Add(history);
            await _context.SaveChangesAsync();

            return history;
        }

        public async Task<DeliveryHistory?> GetDeliveryHistoryByIdAsync(int id)
        {
            return await _context.DeliveryHistories
                .AsNoTracking()
                .Include(dh => dh.DeliveryRoute)
                .Include(dh => dh.Client)
                .FirstOrDefaultAsync(dh => dh.Id == id);
        }

        public async Task<DeliveryHistory?> UpdateDeliveryHistoryAsync(int id, DeliveryHistory updatedHistory)
        {
            if (updatedHistory == null)
                throw new ArgumentNullException(nameof(updatedHistory));

            var existingHistory = await _context.DeliveryHistories.FindAsync(id);

            if (existingHistory == null)
                return null;

            existingHistory.ActualCost = updatedHistory.ActualCost;
            existingHistory.ActualDistanceInKm = updatedHistory.ActualDistanceInKm;
            existingHistory.OrderDate = updatedHistory.OrderDate;
            existingHistory.DeliveredAt = updatedHistory.DeliveredAt;
            existingHistory.Remarks = updatedHistory.Remarks;
            existingHistory.DeliveryRouteId = updatedHistory.DeliveryRouteId;
            existingHistory.ClientId = updatedHistory.ClientId;

            await _context.SaveChangesAsync();

            return existingHistory;
        }

        public async Task<bool> DeleteDeliveryHistoryAsync(int id)
        {
            var history = await _context.DeliveryHistories.FindAsync(id);

            if (history == null)
                return false;

            _context.DeliveryHistories.Remove(history);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
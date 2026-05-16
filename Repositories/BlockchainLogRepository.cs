using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SecureTransparentDataExchange.Models.Repositories
{
    public class BlockchainLogRepository : IBlockchainLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlockchainLogRepository> _logger;

        public BlockchainLogRepository(
            ApplicationDbContext context,
            ILogger<BlockchainLogRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddAsync(BlockchainLog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log), "Blockchain log cannot be null.");

            _logger.LogDebug("Adding new immutable blockchain log: {@Log}", log);

            await _context.BlockchainLogs.AddAsync(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Immutable blockchain log with ID {Id} added successfully.",
                log.Id
            );
        }

        public async Task<List<BlockchainLog>> GetAllAsync()
        {
            _logger.LogDebug("Retrieving all blockchain logs.");

            return await _context.BlockchainLogs
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<BlockchainLog?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving blockchain log with ID {Id}.", id);

            return await _context.BlockchainLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task UpdateAsync(int id, BlockchainLog updatedLog)
        {
            _logger.LogWarning(
                "Attempt to update immutable blockchain log with ID {Id}.",
                id
            );

            throw new NotSupportedException(
                "Blockchain logs are immutable and cannot be updated."
            );
        }

        public Task DeleteAsync(int id)
        {
            _logger.LogWarning(
                "Attempt to delete immutable blockchain log with ID {Id}.",
                id
            );

            throw new NotSupportedException(
                "Blockchain logs are immutable and cannot be deleted."
            );
        }
    }
}
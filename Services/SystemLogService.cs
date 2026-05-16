using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services
{
    public class SystemLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SystemLogService> _logger;

        public SystemLogService(ApplicationDbContext context, ILogger<SystemLogService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieve all system logs.
        /// </summary>
        public async Task<(bool success, string message, List<SystemLog>? logs)> GetSystemLogsAsync()
        {
            try
            {
                var logs = await _context.SystemLogs.AsNoTracking().ToListAsync();

                if (!logs.Any())
                {
                    _logger.LogWarning("No system logs found.");
                    return (false, "No system logs found.", null);
                }

                _logger.LogInformation("Successfully retrieved all system logs.");
                return (true, "System logs retrieved successfully.", logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system logs.");
                return (false, "Error retrieving system logs.", null);
            }
        }

        /// <summary>
        /// Retrieve a system log by ID.
        /// </summary>
        public async Task<(bool success, string message, SystemLog? log)> GetSystemLogByIdAsync(int logId)
        {
            try
            {
                var log = await _context.SystemLogs.FindAsync(logId);

                if (log == null)
                {
                    return (false, $"System log with ID {logId} not found.", null);
                }

                return (true, "System log retrieved successfully.", log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving system log with ID {logId}.");
                return (false, $"Error retrieving system log with ID {logId}.", null);
            }
        }

        /// <summary>
        /// Delete a system log by ID.
        /// </summary>
        public async Task<(bool success, string message)> DeleteSystemLogAsync(int logId)
        {
            try
            {
                var log = await _context.SystemLogs.FindAsync(logId);

                if (log == null)
                {
                    return (false, $"System log with ID {logId} not found.");
                }

                _context.SystemLogs.Remove(log);
                await _context.SaveChangesAsync();

                return (true, "System log deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting system log with ID {logId}.");
                return (false, $"Error deleting system log with ID {logId}.");
            }
        }

        /// <summary>
        /// Add a new system log.
        /// </summary>
        public async Task<(bool success, string message)> AddSystemLogAsync(SystemLog log)
        {
            try
            {
                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"System log added with ID {log.Id}.");
                return (true, "System log added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding system log.");
                return (false, "An error occurred while adding the system log.");
            }
        }

        /// <summary>
        /// Update an existing system log.
        /// </summary>
        public async Task<(bool success, string message)> UpdateSystemLogAsync(SystemLog log)
        {
            try
            {
                var existingLog = await _context.SystemLogs.FindAsync(log.Id);
                if (existingLog == null)
                {
                    return (false, $"System log with ID {log.Id} not found.");
                }

                existingLog.Message = log.Message;
                existingLog.Timestamp = log.Timestamp; // Replace CreatedAt with Timestamp

                await _context.SaveChangesAsync();
                _logger.LogInformation($"System log with ID {log.Id} updated.");
                return (true, "System log updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system log.");
                return (false, "An error occurred while updating the system log.");
            }
        }
    }
}

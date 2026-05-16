using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(
            ApplicationDbContext context,
            ILogger<AuditLogService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Базовый метод
        public async Task LogAsync(
            string action,
            string user,
            LogType? type = null,
            int? loginId = null,
            int? appUserId = null)
        {
            var entry = new AuditLog
            {
                Action = action,
                User = user,
                Type = type,
                LoginId = loginId,
                AppUserId = appUserId,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(entry);      // <--- IMPORTANT: AuditLogs
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "🔍 [AUDIT] {Timestamp}: {Action} by {User} (LoginId={LoginId}, AppUserId={AppUserId}, Type={Type})",
                entry.Timestamp,
                entry.Action,
                entry.User,
                entry.LoginId,
                entry.AppUserId,
                entry.Type?.ToString() ?? "None"
            );
        }

        public Task LogInfoAsync(string action, string user, int? loginId = null, int? appUserId = null) =>
            LogAsync(action, user, LogType.Info, loginId, appUserId);

        public Task LogWarningAsync(string action, string user, int? loginId = null, int? appUserId = null) =>
            LogAsync(action, user, LogType.Warning, loginId, appUserId);

        public Task LogErrorAsync(string action, string user, int? loginId = null, int? appUserId = null) =>
            LogAsync(action, user, LogType.Error, loginId, appUserId);
    }
}

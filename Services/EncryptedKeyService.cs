using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Services
{
    public sealed class EncryptedKeyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EncryptedKeyService> _logger;

        public EncryptedKeyService(
            ApplicationDbContext context,
            ILogger<EncryptedKeyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public EncryptionKey? GetActive()
        {
            return _context.EncryptionKeys
                .AsNoTracking()
                .FirstOrDefault(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow);
        }

        public void RotateActive()
        {
            var key = _context.EncryptionKeys
                .FirstOrDefault(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow);

            if (key == null)
                throw new InvalidOperationException("No active encryption key found.");

            key.RotateKeys();

            _context.SaveChanges();

            _logger.LogInformation(
                "System encryption key with ID {Id} rotated.",
                key.Id
            );
        }

        public IReadOnlyCollection<EncryptionKey> GetAll()
        {
            return _context.EncryptionKeys
                .AsNoTracking()
                .ToList();
        }
    }
}
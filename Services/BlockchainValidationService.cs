using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using System.Security.Cryptography;
using System.Text;

namespace SecureTransparentDataExchange.Services
{
    public class BlockchainValidationService
    {
        private readonly ApplicationDbContext _db;

        public BlockchainValidationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ValidateShipmentChain(int shipmentId)
        {
            var blocks = await _db.BlockchainLogs
                .Where(b => b.ShipmentId == shipmentId)
                .OrderBy(b => b.Id)
                .ToListAsync();

            if (blocks.Count == 0)
                return true;

            for (int i = 1; i < blocks.Count; i++)
            {
                var current = blocks[i];
                var previous = blocks[i - 1];

                // 1️⃣ проверка связи блоков
                if (current.PreviousBlockHash != previous.BlockHash)
                    return false;

                // 2️⃣ пересчет hash
                using var sha = SHA256.Create();
                var raw = $"{current.PreviousBlockHash}-{current.Data}";
                var bytes = Encoding.UTF8.GetBytes(raw);
                var recalculatedHash = Convert.ToBase64String(sha.ComputeHash(bytes));

                if (current.BlockHash != recalculatedHash)
                    return false;
            }

            return true;
        }
    }
}
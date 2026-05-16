using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Repositories;

namespace SecureTransparentDataExchange.Services.Blockchain
{
    public sealed class LedgerFacade
    {
        private readonly BlockchainLogService _logs;

        public LedgerFacade(BlockchainLogService logs)
        {
            _logs = logs;
        }

        public async Task<string> AppendAsync(
            int shipmentId,
            int loginId,
            string entity,
            object payload,
            string? description = null)
        {
            if (string.IsNullOrWhiteSpace(entity))
                throw new ArgumentException("Entity name is required.", nameof(entity));

            IEnumerable<BlockchainLog> all = await _logs.GetAllLogsAsync();

            BlockchainLog? lastForShipment = all
                .Where(x => x.ShipmentId == shipmentId)
                .OrderBy(x => x.Timestamp)
                .LastOrDefault();

            string prevHash = lastForShipment?.BlockHash ?? string.Empty;

            string payloadJson = JsonSerializer.Serialize(payload);

            string toHash = string.IsNullOrEmpty(prevHash)
                ? payloadJson
                : $"{payloadJson}|{prevHash}";

            string currHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(toHash))
            );

            var block = new BlockchainLog
            {
                ShipmentId = shipmentId,
                LoginId = loginId,
                PreviousBlockHash = string.IsNullOrEmpty(prevHash) ? "-" : prevHash,
                BlockHash = currHash,
                Data = JsonSerializer.Serialize(new { entity, payload }),
                Timestamp = DateTime.UtcNow,
                Description = description
            };

            await _logs.AddLogAsync(block);

            return currHash;
        }
    }
}
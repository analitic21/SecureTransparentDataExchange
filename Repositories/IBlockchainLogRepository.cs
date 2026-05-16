using System.Collections.Generic;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Repositories;

namespace SecureTransparentDataExchange.Models.Repositories
{
    public interface IBlockchainLogRepository
    {
        Task AddAsync(BlockchainLog log);
        Task<List<BlockchainLog>> GetAllAsync();
        Task<BlockchainLog?> GetByIdAsync(int id);
        Task UpdateAsync(int id, BlockchainLog updatedLog);
        Task DeleteAsync(int id);
    }
}

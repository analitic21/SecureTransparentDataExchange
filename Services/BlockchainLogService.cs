using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Models.Repositories
{
    public class BlockchainLogService
    {
        private readonly IBlockchainLogRepository _repository;

        public BlockchainLogService(IBlockchainLogRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task AddLogAsync(BlockchainLog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            await _repository.AddAsync(log);
        }

        public async Task<IEnumerable<BlockchainLog>> GetAllLogsAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "An error occurred while retrieving blockchain logs.",
                    ex
                );
            }
        }

        public async Task<BlockchainLog> GetBlockchainLogByIdAsync(int id)
        {
            try
            {
                var blockchainLog = await _repository.GetByIdAsync(id);

                if (blockchainLog == null)
                    throw new KeyNotFoundException(
                        $"Blockchain log with ID {id} not found."
                    );

                return blockchainLog;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An error occurred while retrieving blockchain log with ID {id}.",
                    ex
                );
            }
        }

        public async Task<(bool Valid, string Message)> ValidateChainAsync()
        {
            var logs = (await _repository.GetAllAsync())
                .OrderBy(x => x.Id)
                .ToList();

            if (logs.Count == 0)
                return (true, "Blockchain is empty.");

            for (int i = 1; i < logs.Count; i++)
            {
                var previous = logs[i - 1];
                var current = logs[i];

                if (current.PreviousBlockHash != previous.BlockHash)
                {
                    return (
                        false,
                        $"Blockchain chain is broken at block ID {current.Id}."
                    );
                }
            }

            return (true, "Blockchain chain is valid.");
        }
    }
}
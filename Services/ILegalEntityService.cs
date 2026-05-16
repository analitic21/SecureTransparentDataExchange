using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.API.LegalEntities;
using SecureTransparentDataExchange.Models.Orders;

namespace SecureTransparentDataExchange.Services
{
    public interface ILegalEntityService
    {
        Task<LegalEntity?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<LegalEntity?> GetByLoginIdAsync(int loginId, CancellationToken ct = default);

        Task<bool> ExistsByLoginIdAsync(int loginId, CancellationToken ct = default);

        Task<List<LegalEntity>> GetAllAsync(CancellationToken ct = default);
        Task<LegalEntity> CreateAsync(
            LegalEntityCreateDto dto,
            int loginId,
            CancellationToken ct = default);

        Task<LegalEntity?> UpdateAsync(
            LegalEntityUpdateDto dto,
            int userId,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
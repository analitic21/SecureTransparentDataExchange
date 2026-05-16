using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.API.LegalEntities;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Services
{
    public class LegalEntityService : ILegalEntityService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<LegalEntityService> _log;

        public LegalEntityService(ApplicationDbContext db, ILogger<LegalEntityService> log)
        {
            _db = db;
            _log = log;
        }

        public Task<LegalEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.LegalEntities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<LegalEntity?> GetByLoginIdAsync(int loginId, CancellationToken ct = default) =>
            _db.LegalEntities.AsNoTracking().FirstOrDefaultAsync(x => x.LoginId == loginId, ct);
        public Task<bool> ExistsByLoginIdAsync(int loginId, CancellationToken ct = default) =>
    _db.LegalEntities.AsNoTracking().AnyAsync(x => x.LoginId == loginId, ct);
        public async Task<LegalEntity> CreateAsync(
            LegalEntityCreateDto dto,
            int loginId,
            CancellationToken ct = default)
        {
            var exists = await _db.LegalEntities.AnyAsync(x => x.LoginId == loginId, ct);
            if (exists)
                throw new InvalidOperationException("Legal entity already exists.");

            var entity = new LegalEntity
            {
                LoginId = loginId,
                CompanyName = Required(dto.CompanyName),
                TaxId = dto.TaxId?.Trim(),
                RegistrationNumber = dto.RegistrationNumber?.Trim(),
                ContactPerson = dto.ContactPerson?.Trim(),
                ContactPosition = dto.ContactPosition?.Trim(),
                CompanyPhone = dto.CompanyPhone?.Trim(),
                ManualCountry = Required(dto.ManualCountry),
                ManualCity = Required(dto.ManualCity),
                ManualPostalCode = Required(dto.ManualPostalCode),
                ManualAddress = Required(dto.ManualAddress),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.LegalEntities.Add(entity);
            await _db.SaveChangesAsync(ct);

            return entity;
        }

        // ✅ FIXED UPDATE
        public async Task<LegalEntity?> UpdateAsync(
    LegalEntityUpdateDto dto,
    int userId,
    CancellationToken ct = default)
        {
            var entity = await _db.LegalEntities
                .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

            if (entity == null)
                return null;

            if (entity.LoginId != userId)
                throw new UnauthorizedAccessException();

            entity.CompanyName = Required(dto.CompanyName);
            entity.TaxId = dto.TaxId?.Trim();
            entity.RegistrationNumber = dto.RegistrationNumber?.Trim();
            entity.ContactPerson = dto.ContactPerson?.Trim();
            entity.ContactPosition = dto.ContactPosition?.Trim();
            entity.CompanyPhone = dto.CompanyPhone?.Trim();
            entity.ManualCountry = Required(dto.ManualCountry);
            entity.ManualCity = Required(dto.ManualCity);
            entity.ManualPostalCode = Required(dto.ManualPostalCode);
            entity.ManualAddress = Required(dto.ManualAddress);
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return entity;
        }
        public async Task<List<LegalEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.LegalEntities
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.LegalEntities.FindAsync(new object[] { id }, ct);
            if (entity == null) return false;

            _db.LegalEntities.Remove(entity);
            await _db.SaveChangesAsync(ct);

            return true;
        }

        private static string Required(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Required field is missing");

            return value.Trim();
        }
    }
}

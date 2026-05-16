using SecureTransparentDataExchange.Data;
using  SecureTransparentDataExchange.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SecureTransparentDataExchange.Services
{
    public class PostalCodeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostalCodeService> _logger;

        public PostalCodeService(
            ApplicationDbContext context,
            ILogger<PostalCodeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PostalCodeDTO>> GetByCityIdAsync(int cityId)
        {
            _logger.LogInformation("Fetching postal codes for city {CityId}", cityId);

            return await _context.PostalCodes
                .AsNoTracking()
                .Where(pc => pc.CityId == cityId)
                .OrderBy(pc => pc.Code)
                .Select(pc => new PostalCodeDTO
                {
                    Id = pc.Id,
                    Code = pc.Code ?? string.Empty
                })
                .ToListAsync();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Services.Security;

namespace SecureTransparentDataExchange.Services
{
    public class QRCodeStorageService
    {
        private readonly ApplicationDbContext _context;
        private readonly QRCodeService _qrCodeService;

        public QRCodeStorageService(
            ApplicationDbContext context,
            QRCodeService qrCodeService)
        {
            _context = context;
            _qrCodeService = qrCodeService;
        }

        // ======================================
        // READ
        // ======================================

        public async Task<List<QRCodeModel>> GetAllAsync()
        {
            return await _context.QRCodeModels
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<QRCodeModel?> GetByIdAsync(int id)
        {
            return await _context.QRCodeModels
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // ======================================
        // CREATE
        // ======================================
        public async Task<QRCodeModel> CreateAsync(QRCodeCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SecretKey))
                throw new ArgumentException("SecretKey is required");

            var otpauth = _qrCodeService.GenerateOtpAuthUri(dto.Email, dto.SecretKey);
            var base64 = _qrCodeService.GenerateQrCodeBase64(otpauth);

            var entity = new QRCodeModel
            {
                Email = dto.Email,
                SecretKey = dto.SecretKey,
                Content = otpauth,
                QrCodeBase64 = base64,
                CreatedAt = DateTime.UtcNow
            };

            _context.QRCodeModels.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }


        // ======================================
        // DELETE
        // ======================================

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.QRCodeModels.FindAsync(id);
            if (entity == null)
                return false;

            _context.QRCodeModels.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

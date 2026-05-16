using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using  SecureTransparentDataExchange.DTOs;

namespace SecureTransparentDataExchange.Services
{
    public class ParcelService
    {
        private readonly ApplicationDbContext _context;

        public ParcelService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== GET ALL ==================
        public async Task<List<ParcelDto>> GetAllAsync()
        {
            return await _context.Parcels
                .Select(p => new ParcelDto
                {
                    Id = p.Id,
                    TrackingNumber = p.TrackingNumber,
                    Sender = p.Sender,
                    Receiver = p.Receiver,
                    Status = p.Status,
                    ShipmentId = p.ShipmentId,
                    CargoTypeId = p.CargoTypeId,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        // ================== GET BY ID ==================
        public async Task<ParcelDto?> GetByIdAsync(int id)
        {
            return await _context.Parcels
                .Where(p => p.Id == id)
                .Select(p => new ParcelDto
                {
                    Id = p.Id,
                    TrackingNumber = p.TrackingNumber,
                    Sender = p.Sender,
                    Receiver = p.Receiver,
                    Status = p.Status,
                    ShipmentId = p.ShipmentId,
                    CargoTypeId = p.CargoTypeId,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        // ================== CREATE ==================
        public async Task<ParcelDto> CreateAsync(CreateParcelRequest request)
        {
            var parcel = new Parcel
            {
                TrackingNumber = request.TrackingNumber ?? string.Empty,
                Sender = request.Sender ?? string.Empty,
                Receiver = request.Receiver ?? string.Empty,
                Status = request.Status ?? string.Empty,
                ShipmentId = request.ShipmentId,
                CargoTypeId = request.CargoTypeId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Parcels.Add(parcel);
            await _context.SaveChangesAsync();

            return new ParcelDto
            {
                Id = parcel.Id,
                TrackingNumber = parcel.TrackingNumber,
                Sender = parcel.Sender,
                Receiver = parcel.Receiver,
                Status = parcel.Status,
                ShipmentId = parcel.ShipmentId,
                CargoTypeId = parcel.CargoTypeId,
                CreatedAt = parcel.CreatedAt
            };
        }

        // ================== UPDATE ==================
        public async Task<ParcelDto?> UpdateAsync(int id, UpdateParcelRequest request)
        {
            var parcel = await _context.Parcels.FindAsync(id);

            if (parcel == null)
                return null;

           parcel.Sender = request.Sender ?? string.Empty;
parcel.Receiver = request.Receiver ?? string.Empty;
parcel.Status = request.Status ?? string.Empty;
parcel.ShipmentId = request.ShipmentId;
parcel.CargoTypeId = request.CargoTypeId;
parcel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ParcelDto
            {
                Id = parcel.Id,
                TrackingNumber = parcel.TrackingNumber,
                Sender = parcel.Sender,
                Receiver = parcel.Receiver,
                Status = parcel.Status,
                ShipmentId = parcel.ShipmentId,
                CargoTypeId = parcel.CargoTypeId,
                CreatedAt = parcel.CreatedAt
            };
        }

        // ================== DELETE ==================
        public async Task<bool> DeleteAsync(int id)
        {
            var parcel = await _context.Parcels.FindAsync(id);

            if (parcel == null)
                return false;

            _context.Parcels.Remove(parcel);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
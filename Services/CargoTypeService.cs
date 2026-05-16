using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services
{
    public class CargoTypeService
    {
        private readonly ApplicationDbContext _context;

        public CargoTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all cargo types
        public async Task<List<CargoType>> GetAllCargoTypesAsync()
        {
            return await _context.CargoTypes.ToListAsync();
        }

        // Get cargo type by ID
        public async Task<CargoType?> GetCargoTypeAsync(int id)
        {
            // Return null if cargo type not found
            return await _context.CargoTypes.FindAsync(id);
        }

        // Add new cargo type
        public async Task<CargoType> AddCargoTypeAsync(CargoType cargoType)
        {
            if (cargoType == null)
            {
                throw new ArgumentNullException(nameof(cargoType), "Cargo type cannot be null");
            }

            _context.CargoTypes.Add(cargoType);
            await _context.SaveChangesAsync();
            return cargoType;
        }

        // Update cargo type 
        public async Task<CargoType> UpdateCargoTypeAsync(CargoType cargoType)
        {
            if (cargoType == null)
            {
                throw new ArgumentNullException(nameof(cargoType), "Cargo type cannot be null");
            }

            _context.CargoTypes.Update(cargoType);
            await _context.SaveChangesAsync();
            return cargoType;
        }

        // Delete cargo type 
        public async Task<bool> DeleteCargoTypeAsync(int id)
        {
            var cargoType = await GetCargoTypeAsync(id);
            if (cargoType == null)
            {
                return false;
            }

            _context.CargoTypes.Remove(cargoType);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
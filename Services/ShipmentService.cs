using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models.Shipping;

namespace SecureTransparentDataExchange.Services
{
    public class ShipmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShipmentService> _logger;
        private readonly ShipmentStatusManager _statusManager;
        private readonly ClientService _clientService;

        public ShipmentService(
            ApplicationDbContext context,
            ILogger<ShipmentService> logger,
            ShipmentStatusManager statusManager,
            ClientService clientService)
        {
            _context = context;
            _logger = logger;
            _statusManager = statusManager;
            _clientService = clientService;
        }

        // =========================================================
        // CREATE SHIPMENT
        // =========================================================
        public async Task<(bool success, string message, Shipment? shipment)>
    CreateShipmentAsync(
        Shipment shipment,
        int loginId,
        bool isStaff = false,
        int? clientId = null)
        {
            try
            {
                if (isStaff && clientId.HasValue)
                {
                    var clientExists = await _context.Clients
                        .AnyAsync(c => c.Id == clientId.Value);

                    if (!clientExists)
                        return (false, "Selected client not found.", null);

                    shipment.ClientId = clientId.Value;
                }
                else
                {
                    var client = await _clientService
                        .GetOrCreateClientAsync(loginId, "Client", "User");

                    if (client == null)
                        return (false, "Client profile not found.", null);

                    shipment.ClientId = client.Id;
                }

                _statusManager.SetCreated(shipment);
                shipment.TrackingNumber = GenerateTrackingNumber();

                _context.Shipments.Add(shipment);
                await _context.SaveChangesAsync();

                return (true, "Shipment created successfully.", shipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shipment");
                return (false, "Error creating shipment", null);
            }
        }
        // =========================================================
        // ADD ITEM / PARCEL TO SHIPMENT
        // =========================================================
        public async Task<Parcel> AddItemAsync(int shipmentId, string name, int quantity)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Parcels)
                .FirstOrDefaultAsync(s => s.Id == shipmentId);

            if (shipment == null)
                throw new InvalidOperationException("Shipment not found.");

            var parcel = new Parcel
            {
                ShipmentId = shipmentId,
                TrackingNumber = shipment.TrackingNumber,
                Sender = "Employee",
                Receiver = name,
                Status = $"Added item x{quantity}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Parcels.Add(parcel);
            await _context.SaveChangesAsync();

            return parcel;
        }
        // =========================================================
        // GET SHIPMENTS BY LOGIN
        // =========================================================
        public async Task<List<Shipment>> GetShipmentsByLoginIdAsync(int loginId)
        {
            return await _context.Shipments
                .Include(s => s.Client)
                .Where(s => s.Client != null && s.Client.LoginId == loginId)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }
        public async Task<List<Shipment>> GetAllShipmentsAsync()
        {
            return await _context.Shipments
                .AsNoTracking()
                .Include(s => s.Client)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }
        public async Task<List<object>> GetAllShipmentDtosAsync()
        {
            var shipments = await _context.Shipments
                .AsNoTracking()
                .Include(s => s.Client)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            return shipments.Select(s => new
            {
                id = s.Id,
                trackingNumber = s.TrackingNumber,
                status = s.Status.ToString(),
                paymentStatus = s.PaymentStatus.ToString(),
                createdAt = s.CreatedAt,
                clientId = s.ClientId,
                clientName = s.Client != null
                    ? $"{s.Client.Name} {s.Client.LastName}"
                    : null
            })
            .Cast<object>()
            .ToList();
        }
        // =========================================================
        // OWNERSHIP CHECK
        // =========================================================
        public async Task<bool> UserOwnsShipmentAsync(int shipmentId, int loginId)
        {
            return await _context.Shipments
                .Include(s => s.Client)
                .AnyAsync(s =>
                    s.Id == shipmentId &&
                    s.Client != null &&
                    s.Client.LoginId == loginId);
        }

        // =========================================================
        // GET BY ID
        // =========================================================
        public async Task<Shipment?> GetByIdAsync(int id)
        {
            return await _context.Shipments
                .Include(s => s.Client)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // =========================================================
        // UPDATE (ONLY CREATED)
        // =========================================================
        public async Task<(bool success, string message)>
            UpdateDraftShipmentAsync(int shipmentId, Shipment updated)
        {
            var shipment = await _context.Shipments.FindAsync(shipmentId);
            if (shipment == null)
                return (false, "Shipment not found.");

            if (shipment.Status != ShipmentStatus.Created)
                return (false, "Only shipments in Created status can be edited.");

            shipment.PaymentStatus = updated.PaymentStatus;

            await _context.SaveChangesAsync();
            return (true, "Shipment updated.");
        }
        // =========================================================
        // GET BY TRACKING NUMBER
        // =========================================================
        public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, int loginId)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return null;

            trackingNumber = trackingNumber.Trim();

            return await _context.Shipments
                .Include(s => s.Client)
                .FirstOrDefaultAsync(s =>
                    s.TrackingNumber == trackingNumber &&
                    s.Client != null &&
                    s.Client.LoginId == loginId);
        }

        // =========================================================
        // CANCEL
        // =========================================================
        public async Task<(bool success, string message)>
            CancelShipmentAsync(int shipmentId)
        {
            var shipment = await _context.Shipments.FindAsync(shipmentId);
            if (shipment == null)
                return (false, "Shipment not found.");

            if (shipment.Status == ShipmentStatus.Canceled)
                return (false, "Shipment already canceled.");

            _statusManager.SetCancelled(shipment);
            await _context.SaveChangesAsync();

            return (true, "Shipment canceled.");
        }

        // =========================================================
        // TRACKING NUMBER
        // =========================================================
        private static string GenerateTrackingNumber()
        {
            return $"TRK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Services.AI;
using SecureTransparentDataExchange.Services.Realtime;

namespace SecureTransparentDataExchange.Services
{
    public class IoTDeviceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IoTWebSocketManager _ws;
        private readonly MovementPredictionService _movement;
        private readonly DeliveryEtaService _etaService;
        private readonly DeliveryDestinationResolver _destinationResolver;
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;
        public IoTDeviceService(
    ApplicationDbContext context,
    IoTWebSocketManager ws,
    MovementPredictionService movement,
    DeliveryEtaService etaService,
    DeliveryDestinationResolver destinationResolver,
    EmailService emailService,
    SmsService smsService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ws = ws ?? throw new ArgumentNullException(nameof(ws));
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));
            _etaService = etaService ?? throw new ArgumentNullException(nameof(etaService));
            _destinationResolver = destinationResolver ?? throw new ArgumentNullException(nameof(destinationResolver));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        }

        // ================= BASIC =================
        public async Task<IoTDevice?> GetByIdAsync(int id)
        {
            return await _context.IoTDevices
                .Include(d => d.Parcel!)
                    .ThenInclude(p => p.Shipment!)
                        .ThenInclude(s => s.Client!)
                            .ThenInclude(c => c.Login)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        public async Task<IoTDevice?> GetByTrackingNumberAsync(string trackingNumber)
        {
            return await _context.IoTDevices
                .Include(d => d.Parcel!)
                    .ThenInclude(p => p.Shipment)
                .FirstOrDefaultAsync(d => d.TrackingNumber == trackingNumber);
        }
        public async Task<List<IoTDevice>> GetAllAsync()
        {
            return await _context.IoTDevices
                .Include(d => d.Parcel)
                .ToListAsync();
        }

        public async Task<IoTDevice> CreateAsync(IoTDevice device)
        {
            device.LastUpdated = DateTime.UtcNow;
            _context.IoTDevices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<IoTDevice?> UpdateAsync(int id, IoTDevice updated)
        {
            var device = await GetByIdAsync(id);
            if (device == null)
                return null;

            device.TrackingNumber = updated.TrackingNumber;
            device.Latitude = updated.Latitude;
            device.Longitude = updated.Longitude;
            device.ParcelId = updated.ParcelId;
            device.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return device;
        }

        // ================= TELEMETRY =================
        public async Task<bool> UpdateTelemetryAsync(int id, double lat, double lng)
        {
            if (lat < -90 || lat > 90 || lng < -180 || lng > 180)
                throw new ArgumentException("Invalid coordinates");

            var device = await GetByIdAsync(id);
            if (device == null)
                return false;

            var now = DateTime.UtcNow;

            device.Latitude = lat;
            device.Longitude = lng;
            device.LastUpdated = now;

            await _context.SaveChangesAsync();

            // ===== AI =====
            _movement.AddPoint(id, lat, lng, now);
            var prediction = _movement.PredictNext(id);
            var stats = _movement.GetStats(id);

            // ===== ETA =====
            var destination = await _destinationResolver.GetDestinationForDeviceAsync(id);

            object? eta = null;
            bool arrived = false;
            string? autoStatus = null;

            if (destination.HasValue)
            {
                var dest = destination.Value;

                eta = _etaService.CalculateEta(
                    lat,
                    lng,
                    dest.lat,
                    dest.lng,
                    stats?.SpeedKmh ?? 0
                );

                arrived = _etaService.IsArrived(
                    lat,
                    lng,
                    dest.lat,
                    dest.lng
                );
            }

            // ===== AUTO STATUS =====
            if (device.Parcel?.Shipment != null)
            {
                var shipment = device.Parcel.Shipment;
                var previousStatus = shipment.Status;

                if (arrived)
                {
                    shipment.Status = ShipmentStatus.Delivered;
                    autoStatus = "Delivered";

                    if (previousStatus != ShipmentStatus.Delivered)
                    {
                        var login = shipment.Client?.Login;

                        if (!string.IsNullOrWhiteSpace(login?.Email))
                        {
                            await _emailService.SendEmailAsync(
                                login.Email,
                                "Parcel Delivered",
                                $"Your parcel {device.TrackingNumber} has been delivered.",
                                null
                            );
                        }

                        if (!string.IsNullOrWhiteSpace(login?.PhoneNumber))
                        {
                            var sms = new SmsModel(
                                login.PhoneNumber,
                                ShipmentStatus.Delivered,
                                device.TrackingNumber ?? string.Empty
                            );

                            await _smsService.SendSmsAsync(sms);
                        }
                    }
                }
                else if (eta is DeliveryEtaResult etaResult &&
                         etaResult.RemainingMinutes.HasValue &&
                         etaResult.RemainingMinutes.Value <= 15)
                {
                    shipment.Status = ShipmentStatus.OutForDelivery;
                    autoStatus = "OutForDelivery";
                }
                else
                {
                    shipment.Status = ShipmentStatus.InTransit;
                    autoStatus = "InTransit";
                }

                await _ws.BroadcastTelemetryAsync(new
                {
                    type = "iot_update",
                    deviceId = id,
                    trackingNumber = device.TrackingNumber,
                    latitude = lat,
                    longitude = lng,
                    updatedAt = now,
                    predicted = prediction,
                    stats,
                    eta,
                    arrived,
                    autoStatus
                }, device.TrackingNumber ?? string.Empty);
            }
            return true;
        }

        // ================= OTHER =================
        public async Task<bool> HeartbeatAsync(int id)
        {
            var device = await GetByIdAsync(id);
            if (device == null)
                return false;

            device.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public bool IsDeviceOnline(IoTDevice device, int minutes = 2)
        {
            return DateTime.UtcNow - device.LastUpdated < TimeSpan.FromMinutes(minutes);
        }

        public async Task<(double lat, double lng, DateTime updated)?> GetLastPositionAsync(int id)
        {
            var device = await GetByIdAsync(id);
            if (device == null)
                return null;

            return (device.Latitude, device.Longitude, device.LastUpdated);
        }

        public async Task<List<(double lat, double lng, DateTime updated)>> GetRouteAsync(int id)
        {
            var device = await GetByIdAsync(id);
            if (device == null)
                return new();

            return new()
            {
                (device.Latitude, device.Longitude, device.LastUpdated)
            };
        }

        public async Task<string?> ExportRouteCsvAsync(int id)
        {
            var route = await GetRouteAsync(id);
            if (route.Count == 0)
                return null;

            var lines = new List<string>
            {
                "Latitude,Longitude,UpdatedAt"
            };

            lines.AddRange(route.Select(r => $"{r.lat},{r.lng},{r.updated:O}"));

            return string.Join(Environment.NewLine, lines);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var device = await GetByIdAsync(id);
            if (device == null)
                return false;

            _context.IoTDevices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByTrackingNumberAsync(string trackingNumber)
        {
            var device = await GetByTrackingNumberAsync(trackingNumber);
            if (device == null)
                return false;

            _context.IoTDevices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
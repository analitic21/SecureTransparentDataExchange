using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Orders;
using SecureTransparentDataExchange.Models.Shipping;
using SecureTransparentDataExchange.Services.Interfaces;

namespace SecureTransparentDataExchange.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly IOrderNumberService _orderNumberService;
        private readonly DeliveryPriceService _deliveryPriceService;

        public OrderService(
            ApplicationDbContext db,
            IOrderNumberService orderNumberService,
            DeliveryPriceService deliveryPriceService)
        {
            _db = db;
            _orderNumberService = orderNumberService;
            _deliveryPriceService = deliveryPriceService;
        }

        public async Task<Order> CreateForCurrentUserAsync(
            int loginId,
            decimal totalAmount,
            string currency = "EUR",
            string? externalOrderNumber = null,
            string source = "Web")
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == loginId);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            if (user.UserType == null)
                throw new InvalidOperationException("User type is not defined.");

            var userType = user.UserType.Value;

            decimal calculatedAmount = totalAmount;
            int? resolvedClientId = null;
            int? resolvedLegalEntityId = null;

            if (userType == UserType.Individual)
            {
                var client = await _db.Clients
                    .FirstOrDefaultAsync(c => c.LoginId == loginId);

                if (client == null)
                    throw new InvalidOperationException("Client profile not found.");

                if (!client.AddressId.HasValue)
                    throw new InvalidOperationException("Client address is required.");

                resolvedClientId = client.Id;

                const int warehouseAddressId = 1;
                int destinationAddressId = client.AddressId.Value;

                var priceResult = await _deliveryPriceService.CalculateDeliveryPriceAsync(
                    warehouseAddressId,
                    destinationAddressId
                );

                if (!priceResult.Success)
                    throw new InvalidOperationException(priceResult.Message);

                calculatedAmount = priceResult.Price;
            }
            else if (userType == UserType.LegalEntity)
            {
                var legalEntity = await _db.LegalEntities
                    .FirstOrDefaultAsync(le => le.LoginId == loginId);

                if (legalEntity == null)
                    throw new InvalidOperationException("Legal entity profile not found.");

                resolvedLegalEntityId = legalEntity.Id;
            }
            else
            {
                throw new InvalidOperationException("Unsupported user type for order creation.");
            }

            var shipment = new Shipment
            {
                ClientId = resolvedClientId,
                TrackingNumber = Guid.NewGuid().ToString("N")[..12].ToUpper(),
                Status = ShipmentStatus.Created,
                PaymentStatus = PaymentStatus.Pending
            };

            _db.Shipments.Add(shipment);
            await _db.SaveChangesAsync();

            var order = new Order
            {
                LoginId = loginId,
                UserType = userType,
                ClientId = resolvedClientId,
                
                ShipmentId = shipment.Id,
                OrderNumber = _orderNumberService.Generate(userType, source),
                TotalAmount = calculatedAmount,
                Currency = currency,
                PaymentStatus = PaymentStatus.Pending,
                ExternalOrderNumber = externalOrderNumber,
                Source = source
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return order;
        }

        public async Task<bool> CancelAsync(int orderId, int loginId)
        {
            var order = await _db.Orders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            if (order.LoginId != loginId)
                throw new UnauthorizedAccessException("Not your order.");

            if (order.Shipment == null)
            {
                _db.Orders.Remove(order);
                await _db.SaveChangesAsync();
                return true;
            }

            if (order.Shipment.Status == ShipmentStatus.Delivered ||
                order.Shipment.Status == ShipmentStatus.Canceled)
                return false;

            order.Shipment.Status = ShipmentStatus.Canceled;
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<Order>> GetActiveOrdersAsync()
        {
            return await _db.Orders
                .Include(o => o.Shipment)
                .Where(o =>
                    o.Shipment != null &&
                    o.Shipment.Status != ShipmentStatus.Delivered &&
                    o.Shipment.Status != ShipmentStatus.Canceled)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByClientIdAsync(int clientId)
        {
            return await _db.Orders
                .Include(o => o.Shipment)
                .Where(o => o.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _db.Orders
                .Include(o => o.Shipment)
                .Include(o => o.Client)
                .Include(o => o.LegalEntity)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        public async Task<List<Order>> GetByLoginIdAsync(int loginId)
        {
            return await _db.Orders
                .Include(o => o.Shipment)
                .Where(o => o.LoginId == loginId)
                .ToListAsync();
        }
    }
}
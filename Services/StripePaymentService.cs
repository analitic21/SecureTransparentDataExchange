using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.Billing;
using SecureTransparentDataExchange.Models.Enums;
using Stripe;

namespace SecureTransparentDataExchange.Services
{
    public class StripePaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(
            ApplicationDbContext db,
            ILogger<StripePaymentService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(bool ok, string message, string? paymentIntentId, string? clientSecret)>
    CreatePaymentIntentAsync(int shipmentId, decimal? requestedAmount = null)
        {
            var shipment = await _db.Shipments
                .FirstOrDefaultAsync(s => s.Id == shipmentId);

            if (shipment == null)
                return (false, "Shipment not found.", null, null);

            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.ShipmentId == shipmentId);

            if (order == null)
                return (false, "Order for this shipment was not found.", null, null);

            var paidAmount = await _db.Payments
                .Where(p =>
                    p.ShipmentId == shipmentId &&
                    p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);

            var remainingAmount = order.TotalAmount - paidAmount;

            if (remainingAmount <= 0)
                return (false, "Order already fully paid.", null, null);

            var amountToPay = requestedAmount.HasValue && requestedAmount.Value > 0
                ? requestedAmount.Value
                : remainingAmount;

            if (amountToPay > remainingAmount)
                return (false, $"Amount exceeds remaining balance. Remaining: {remainingAmount}", null, null);

            var currency = string.IsNullOrWhiteSpace(order.Currency)
                ? "eur"
                : order.Currency.Trim().ToLowerInvariant();

            var payment = new Payment
            {
                ShipmentId = shipmentId,
                OrderId = order.Id,
                Amount = amountToPay,
                Currency = currency.ToUpperInvariant(),
                PaymentMethod = "Card",
                Provider = "Stripe",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            var intentService = new PaymentIntentService();

            var intent = await intentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = ToMinorUnits(amountToPay, currency),
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    ["shipmentId"] = shipmentId.ToString(),
                    ["orderId"] = order.Id.ToString(),
                    ["paymentDbId"] = payment.Id.ToString(),
                    ["partial"] = (amountToPay < remainingAmount).ToString()
                }
            });

            payment.ExternalPaymentId = intent.Id;
            payment.TransactionId = intent.Id;

            await _db.SaveChangesAsync();

            return (true, "OK", intent.Id, intent.ClientSecret);
        }
        private static long ToMinorUnits(decimal amount, string currency)
        {
            return (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        }
    }
}
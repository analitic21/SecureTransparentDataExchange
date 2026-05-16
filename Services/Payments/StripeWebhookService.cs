using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Billing;
using SecureTransparentDataExchange.Options;
using SecureTransparentDataExchange.Services.StateMachines;
using Stripe;

namespace SecureTransparentDataExchange.Services
{
    public class StripeWebhookService
    {
        private readonly ApplicationDbContext _db;
        private readonly StripeOptions _options;
        private readonly ShipmentStateMachine _sm;
        private readonly InvoicePdfService _invoice;
        private readonly EmailService _email;
        private readonly SmsService _sms;

        public StripeWebhookService(
            ApplicationDbContext db,
            IOptions<StripeOptions> options,
            ShipmentStateMachine sm,
            InvoicePdfService invoice,
            EmailService email,
            SmsService sms)
        {
            _db = db;
            _options = options.Value;
            _sm = sm;
            _invoice = invoice;
            _email = email;
            _sms = sms;
        }

        public async Task HandleAsync(HttpRequest request)
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();

            if (!request.Headers.TryGetValue("Stripe-Signature", out var signature))
                return;

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    _options.WebhookSecret
                );
            }
            catch (StripeException)
            {
                return;
            }

            if (stripeEvent.Type != EventTypes.PaymentIntentSucceeded)
                return;

            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent == null)
                return;

            var payment = await _db.Payments
                .FirstOrDefaultAsync(p =>
                    p.Provider == "Stripe" &&
                    p.ExternalPaymentId == intent.Id);

            if (payment == null || payment.Status == PaymentStatus.Completed)
                return;

            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;

            var paidAmount = await _db.Payments
                .Where(p =>
                    p.ShipmentId == payment.ShipmentId &&
                    p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);

            var order = await _db.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == payment.OrderId ||
                    o.ShipmentId == payment.ShipmentId);

            if (order != null)
            {
                order.PaymentStatus = paidAmount >= order.TotalAmount
                    ? PaymentStatus.Completed
                    : PaymentStatus.PartiallyPaid;
            }

            var charge = intent.LatestCharge as Charge;

            var cardTransactionExists = await _db.CardTransactions
                .AnyAsync(t => t.ProviderTransactionId == intent.Id);

            if (!cardTransactionExists)
            {
                _db.CardTransactions.Add(new CardTransaction
                {
                    ShipmentId = payment.ShipmentId,
                    Amount = payment.Amount,
                    CardNumberMasked = charge?.PaymentMethodDetails?.Card?.Last4 != null
                        ? $"**** **** **** {charge.PaymentMethodDetails.Card.Last4}"
                        : "**** **** **** ****",
                    CardBrand = charge?.PaymentMethodDetails?.Card?.Brand ?? "Unknown",
                    Status = PaymentStatus.Completed,
                    Provider = "Stripe",
                    ProviderTransactionId = intent.Id,
                    ProviderMessage = "Payment completed via Stripe webhook",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
            }

            var shipment = await _db.Shipments
                .Include(s => s.Client!)
                    .ThenInclude(c => c.Login)
                .FirstOrDefaultAsync(s => s.Id == payment.ShipmentId);

            // ===== UPDATE SHIPMENT =====
            if (shipment != null)
            {
                if (order != null && paidAmount >= order.TotalAmount)
                {
                    shipment.PaymentStatus = PaymentStatus.Completed;

                    if (_sm.CanMoveTo(shipment.Status, ShipmentStatus.PaymentCompleted))
                        shipment.Status = ShipmentStatus.PaymentCompleted;
                }
                else
                {
                    shipment.PaymentStatus = PaymentStatus.PartiallyPaid;
                }
            }

            await _db.SaveChangesAsync();

            // ===== BLOCKCHAIN =====
            if (shipment != null)
            {
                var lastBlock = await _db.BlockchainLogs
                    .Where(b => b.ShipmentId == shipment.Id)
                    .OrderByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                var previousHash = lastBlock?.BlockHash ?? "GENESIS";

                var blockData = $"Payment {payment.Id} completed for shipment {shipment.Id}";

                using var sha = System.Security.Cryptography.SHA256.Create();
                var raw = $"{previousHash}-{blockData}";
                var bytes = System.Text.Encoding.UTF8.GetBytes(raw);
                var hash = Convert.ToBase64String(sha.ComputeHash(bytes));

                _db.BlockchainLogs.Add(new BlockchainLog
                {
                    ShipmentId = shipment.Id,
                    LoginId = shipment.Client?.LoginId ?? shipment.Client?.Login?.Id ?? 1,
                    Data = blockData,
                    PreviousBlockHash = previousHash,
                    BlockHash = hash,
                    Timestamp = DateTime.UtcNow,
                    Description = "Payment completed"
                });

                await _db.SaveChangesAsync();
            }

            // ===== POST ACTIONS =====
            var isFullyPaid = order != null && paidAmount >= order.TotalAmount;

            if (shipment != null && isFullyPaid)
            {
                await _invoice.TryGenerateForShipmentAsync(shipment.Id);

                var login = shipment.Client?.Login;

                var email = login?.Email;
                var phone = login?.PhoneNumber;

                if (!string.IsNullOrWhiteSpace(email))
                {
                    await _email.SendEmailAsync(
                        email,
                        "Payment successful",
                        $"Your shipment {shipment.TrackingNumber} has been fully paid successfully."
                    );
                }

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    await _sms.SendSmsAsync(new SmsModel(
                        phone,
                        ShipmentStatus.PaymentCompleted,
                        shipment.TrackingNumber
                    ));
                }
            }
        }
    }
}
           
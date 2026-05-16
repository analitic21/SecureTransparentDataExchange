using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.API.Stripe;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Billing;
using Stripe;

namespace SecureTransparentDataExchange.Controllers.Payments
{
    [ApiController]
    [Route("api/stripe")]
    public class StripePaymentController : ControllerBase
    {
        private readonly StripePaymentService _stripe;
        private readonly ApplicationDbContext _db;

        public StripePaymentController(
        StripePaymentService stripe,
        ApplicationDbContext db)
        {
            _stripe = stripe;
            _db = db;
        }

        // ========================== 
        // CREATE PAYMENT INTENT 
        // The client can pay 
        // ========================== 
        [HttpPost("payment-intent")]
        [Authorize(Roles = "User,Client,Business,LegalEntity,Employee,Admin,Administrator")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreateStripePaymentIntentRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (ok, message, piId, clientSecret) =
                await _stripe.CreatePaymentIntentAsync(req.ShipmentId, req.Amount);

            if (!ok)
                return BadRequest(new { message });

            return Ok(new CreateStripePaymentIntentResponse
            {
                PaymentIntentId = piId!,
                ClientSecret = clientSecret!
            });
        }
        [HttpGet("history")]
        [Authorize(Roles = "Admin,Administrator,Employee")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var payments = await _db.Payments
                .Include(p => p.Refunds)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.OrderId,
                    p.ShipmentId,
                    p.Amount,
                    p.Currency,
                    Status = p.Status.ToString(),
                    p.Provider,
                    p.PaymentMethod,
                    p.ExternalPaymentId,
                    p.CreatedAt,
                    p.CompletedAt,
                    RefundedAmount = p.Refunds.Sum(r => r.Amount),
                    Refunds = p.Refunds.Select(r => new
                    {
                        r.Id,
                        r.Amount,
                        r.Currency,
                        r.StripeRefundId,
                        Status = r.Status.ToString(),
                        r.CreatedAt
                    })
                })
                .ToListAsync();

            return Ok(payments);
        }
        [HttpGet("analytics")]
        [Authorize(Roles = "Admin,Administrator,Employee")]
        public async Task<IActionResult> GetBillingAnalytics()
        {
            var payments = await _db.Payments
                .OrderBy(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Amount,
                    p.Currency,
                    Status = p.Status.ToString(),
                    p.CreatedAt,
                    p.CompletedAt,
                    p.ShipmentId,
                    p.OrderId
                })
                .ToListAsync();

            var totalRevenue = payments
                .Where(p => p.Status == "Completed")
                .Sum(p => p.Amount);

            var pendingAmount = payments
                .Where(p => p.Status == "Pending" || p.Status == "PartiallyPaid")
                .Sum(p => p.Amount);

            var monthly = payments
                .Where(p => p.Status == "Completed")
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();
            var suspicious = payments
    .Where(p => p.Amount > 1000 || p.Status == "Failed")
    .Select(p => new
    {
        p.Id,
        p.Amount,
        Reason = p.Amount > 1000 ? "High amount" : "Failed payment"
    })
    .ToList();
            return Ok(new
            {
                TotalRevenue = totalRevenue,
                PendingAmount = pendingAmount,
                TotalPayments = payments.Count,
                CompletedPayments = payments.Count(p => p.Status == "Completed"),
                Monthly = monthly,
                Suspicious = suspicious
            });
        }
        [HttpGet("payments/full")]
        [Authorize(Roles = "Admin,Administrator,Employee")]
        public async Task<IActionResult> GetFullPayments()
        {
            var payments = await _db.Payments
                .Include(p => p.Shipment)
                .Include(p => p.Invoice)
                .Include(p => p.ProcessedByEmployee)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Amount,
                    p.Currency,
                    Status = p.Status.ToString(),
                    p.Provider,
                    p.PaymentMethod,
                    p.CreatedAt,
                    p.CompletedAt,

                    shipmentId = p.ShipmentId,
                    trackingNumber = p.Shipment.TrackingNumber,

                    invoiceId = p.InvoiceId,

                    employee = p.ProcessedByEmployee != null
                        ? p.ProcessedByEmployee.Name
                        : null
                })
                .ToListAsync();

            return Ok(payments);
        }
        // ========================== 
        // GET PAYMENT STATUS 
        // Any authorized user 
        // ========================== 
        [HttpGet("status/{paymentIntentId}")]
        [Authorize]
        public async Task<IActionResult> GetStatus(string paymentIntentId)
        {
            var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == paymentIntentId);

            if (payment == null)
                return NotFound();

            return Ok(new { status = payment.Status.ToString() });
        }

        // ========================== 
        // GET ALL PAYMENTS 
        // Admin/employee only 
        // ========================== 
        [HttpGet("payments")]
        [Authorize(Roles = "Admin,Administrator,Employee")]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _db.Payments
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.OrderId,
                p.ShipmentId,
                p.Amount,
                p.Currency,
                p.Provider,
                p.PaymentMethod,
                Status = p.Status.ToString(),
                p.ExternalPaymentId,
                p.CreatedAt,
                p.CompletedAt
            })
            .ToListAsync();

            return Ok(payments);
        }
    }
}
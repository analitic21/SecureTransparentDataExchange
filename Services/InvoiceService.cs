using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.API.Invoices;
using SecureTransparentDataExchange.Models.Billing;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Orders;

namespace SecureTransparentDataExchange.Services
{
    public class InvoiceService(ApplicationDbContext db, ILogger<InvoiceService> logger) : IInvoiceService
    {
        private readonly ApplicationDbContext _db = db;
        private readonly ILogger<InvoiceService> _logger = logger;


        public async Task<Invoice> CreateAsync(
    CreateInvoiceRequest req,
    int userId,
    CancellationToken ct = default)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (string.IsNullOrWhiteSpace(req.Number))
                throw new ArgumentException("Invoice number is required.", nameof(req.Number));

            var inv = new Invoice
            {
                Number = req.Number.Trim(),
                UserId = userId,
                Amount = req.Amount,
                PaidAmount = 0m,
                Currency = string.IsNullOrWhiteSpace(req.Currency) ? "EUR" : req.Currency!,
                CreatedAt = DateTime.UtcNow,
                DueDate = req.DueDate ?? DateTime.UtcNow.AddDays(7),
                Description = req.Description,
                Status = InvoiceStatus.Pending
            };

            await _db.Invoices.AddAsync(inv, ct);
            await _db.SaveChangesAsync(ct);

            return inv;
        }
        public async Task<Invoice?> GetAsync(int id, CancellationToken ct = default) =>
            await _db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);

        public async Task<List<Invoice>> GetForUserAsync(int userId, CancellationToken ct = default) =>
            await _db.Invoices.AsNoTracking()
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(ct);

        public async Task<List<Invoice>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            return await _db.Invoices.AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<bool> UpdateAsync(UpdateInvoiceRequest req, CancellationToken ct = default)
        {
            var inv = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == req.Id, ct);
            if (inv is null) return false;

            if (!string.IsNullOrWhiteSpace(req.Description)) inv.Description = req.Description;
            if (req.Amount.HasValue) inv.Amount = req.Amount.Value;
            if (req.DueDate.HasValue) inv.DueDate = req.DueDate.Value;
            if (!string.IsNullOrWhiteSpace(req.Currency)) inv.Currency = req.Currency!;
            if (req.Status.HasValue) inv.Status = req.Status.Value;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> MarkPaidAsync(int id, CancellationToken ct = default)
        {
            var inv = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (inv is null) return false;

            inv.PaidAmount = inv.Amount;
            inv.Status = InvoiceStatus.Paid;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CancelAsync(int id, CancellationToken ct = default)
        {
            var inv = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (inv is null) return false;

            inv.Status = InvoiceStatus.Cancelled;
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

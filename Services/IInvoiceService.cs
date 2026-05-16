using SecureTransparentDataExchange.Models.API.Invoices;
using SecureTransparentDataExchange.Models.Billing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Models.Enums;


namespace SecureTransparentDataExchange.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateAsync(CreateInvoiceRequest req, int userId, CancellationToken ct = default);
        Task<Invoice?> GetAsync(int id, CancellationToken ct = default);
        Task<List<Invoice>> GetForUserAsync(int userId, CancellationToken ct = default);
        Task<List<Invoice>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

        Task<bool> UpdateAsync(UpdateInvoiceRequest request, CancellationToken ct = default);
        Task<bool> MarkPaidAsync(int id, CancellationToken ct = default);
        Task<bool> CancelAsync(int id, CancellationToken ct = default);
    }
}

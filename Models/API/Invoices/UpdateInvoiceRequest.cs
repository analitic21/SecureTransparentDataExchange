using SecureTransparentDataExchange.Models.Billing;
using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Models.API.Invoices
{
    public class UpdateInvoiceRequest
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public InvoiceStatus? Status { get; set; }
        public string? Currency { get; set; }
    }
}
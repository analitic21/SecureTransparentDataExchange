// Models/API/Invoices/InvoiceDto.cs
using SecureTransparentDataExchange.Models.Billing;

namespace SecureTransparentDataExchange.Models.API.Invoices
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int? UserId { get; set; }
        public string? Description { get; set; }
    }
}

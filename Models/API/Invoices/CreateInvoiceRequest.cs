namespace SecureTransparentDataExchange.Models.API.Invoices
{
    public class CreateInvoiceRequest
    {
        public string Number { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "EUR";
        public DateTime? DueDate { get; set; }
        public string? Description { get; set; }
    }
}
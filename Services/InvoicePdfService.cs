using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Services
{
    public class InvoicePdfService
    {
        private readonly ApplicationDbContext _db;
        private readonly EmailService _email;

        public InvoicePdfService(ApplicationDbContext db, EmailService email)
        {
            _db = db;
            _email = email;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task TryGenerateForShipmentAsync(int shipmentId)
        {
            var shipment = await _db.Shipments
     .Include(s => s.Client)
         .ThenInclude(c => c!.Login)
     .FirstOrDefaultAsync(s => s.Id == shipmentId);

            if (shipment == null)
                return;

            var order = await _db.Orders
                .FirstOrDefaultAsync(o =>
                    o.ShipmentId == shipmentId ||
                    o.Id == shipment.OrderId);

            if (order == null)
                return;

            var invoiceNumber = $"INV-{shipment.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var amount = order.TotalAmount;
            var currency = order.Currency;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Content().Column(col =>
                    {
                        col.Item().Text("INVOICE").FontSize(22).Bold();
                        col.Item().Text($"Invoice: {invoiceNumber}");
                        col.Item().Text($"Order: {order.OrderNumber}");
                        col.Item().Text($"Shipment ID: {shipment.Id}");
                        col.Item().Text($"Tracking: {shipment.TrackingNumber}");
                        col.Item().Text($"Amount: {amount} {currency}");
                        col.Item().Text($"Payment status: {shipment.PaymentStatus}");
                        col.Item().Text($"Created: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
                    });
                });
            }).GeneratePdf();

            var email = shipment.Client?.Login?.Email;

            if (!string.IsNullOrWhiteSpace(email))
            {
                await _email.SendEmailAsync(
                    email,
                    "Your invoice",
                    $"Invoice {invoiceNumber} for shipment {shipment.TrackingNumber} is attached.",
                    pdfBytes
                );
            }
        }
    }
}
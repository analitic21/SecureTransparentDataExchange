using System;

namespace SecureTransparentDataExchange.Models
{
    public class EmailModel
    {
        public string To { get; set; } // Recipient's address (required)
        public string Subject { get; set; } // Email subject
        public string Body { get; set; } // Email body
        public string? QrCodeBase64 { get; set; } // QR code in Base64 (if needed to send)

        // Constructor with parameters
        public EmailModel(string to, string subject, string body, string? qrCodeBase64 = null)
        {
            To = !string.IsNullOrWhiteSpace(to)
            ? to
            : throw new ArgumentNullException(nameof(to), "To cannot be null or empty");

            Subject = !string.IsNullOrWhiteSpace(subject)
            ? subject
            : throw new ArgumentNullException(nameof(subject), "Subject cannot be null or empty");

            Body = !string.IsNullOrWhiteSpace(body)
            ? body
            : throw new ArgumentNullException(nameof(body), "Body cannot be null or empty");

            QrCodeBase64 = qrCodeBase64; // Can be null 
        }

        // Additional method for debugging 
        public void PrintEmailDetails()
        {
            Console.WriteLine($"To: {To}");
            Console.WriteLine($"Subject: {Subject}");
            Console.WriteLine($"Body: {Body}");
            Console.WriteLine($"QR Code Base64: {QrCodeBase64 ?? "None"}");
        }
    }
}
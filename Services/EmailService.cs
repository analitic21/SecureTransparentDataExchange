using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace SecureTransparentDataExchange.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Get SMTP server parameters from configuration
            _smtpServer = "smtp.your-email-provider.com"; // Replace with your server
            _smtpPort = 587; // Usually port 587 for TLS
            _senderEmail = "no-reply@yourdomain.com"; // Replace with your email
        }

        public async Task SendEmailAsync(string email, string subject, string body, byte[]? qrCodeBytes = null)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true; // Enable SSL for secure data transfer
                    client.Credentials = new System.Net.NetworkCredential("your-username", "your-password"); // Add your authentication

                    var from = new MailAddress(_senderEmail);
                    var to = new MailAddress(email);

                    var message = new MailMessage(from, to)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    // If there is a QR code, add it as an attachment
                    if (qrCodeBytes != null)
                    {
                        var qrCodeAttachment = new Attachment(new MemoryStream(qrCodeBytes), "qrCode.png", "image/png");
                        message.Attachments.Add(qrCodeAttachment);
                    }

                    // Send the email
                    await client.SendMailAsync(message);
                    _logger.LogInformation($"Email sent successfully to {email}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
            }
        }
    }
}
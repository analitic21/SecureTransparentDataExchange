using System;
using SecureTransparentDataExchange.Data;
using System.Text.Json.Serialization;

namespace SecureTransparentDataExchange.Models
{
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public object? InternalData { get; set; } // Ignore when JSON serializing

        /// <summary>
        /// Creates a successful payment result.
        /// </summary>
        public static PaymentResult Success(string transactionId, string message = "Payment successful", object? internalData = null)
        {
            return new PaymentResult
            {
                IsSuccess = true,
                Message = message,
                TransactionId = transactionId,
                InternalData = internalData
            };
        }

        /// <summary>
        /// Creates a failed payment result.
        /// </summary>
        public static PaymentResult Failure(string message = "Payment failed")
        {
            return new PaymentResult
            {
                IsSuccess = false,
                Message = message,
                TransactionId = null
            };
        }
    }
}

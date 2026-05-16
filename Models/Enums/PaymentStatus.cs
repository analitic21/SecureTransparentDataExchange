using System;

namespace SecureTransparentDataExchange.Models.Enums
{
    public enum PaymentStatus
    {
        Pending,        // created but not paid
        Authorized,     // card confirmed (optional)
        Completed,      // fully paid
        Failed,         // rejected
        Canceled,       // canceled
        Refunded,       // fully refunded
        PartiallyPaid   // ✅ ADD THIS (critical)
    }
}
using System;
// Models/Billing/InvoiceStatus.cs
namespace SecureTransparentDataExchange.Models.Billing
{
    public enum InvoiceStatus
    {
        Draft = 0,
        Pending = 1,   // posted, awaiting payment
        Paid = 2,
        Cancelled = 3
    }
}


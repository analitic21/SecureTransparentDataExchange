using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Data;

namespace SecureTransparentDataExchange.Services
{
    /// <summary>
    /// Service for automatic payment status change.
    /// </summary>
    public class PaymentStatusManager
    {
        /// <summary>
        /// Sets the "Pending payment" status.
        /// </summary>
        public PaymentStatus SetPending() => PaymentStatus.Pending;

        /// <summary>
        /// Sets the "Paid" status.
        /// </summary>
        public PaymentStatus SetCompleted() => PaymentStatus.Completed;

        /// <summary>
        /// Sets the "Payment error" status.
        /// </summary>
        public PaymentStatus SetFailed() => PaymentStatus.Failed;

        /// <summary>
        /// Sets the status to "Refunded".
        /// </summary>
        public PaymentStatus SetRefunded() => PaymentStatus.Refunded;

        /// <summary>
        /// Automatically changes the status based on actions.
        /// </summary>
        public PaymentStatus UpdatePaymentStatus(bool isPaid, bool isRefunded)
        {
            if (isRefunded) return PaymentStatus.Refunded;
            if (isPaid) return PaymentStatus.Completed;
            return PaymentStatus.Failed;
        }
    }
}

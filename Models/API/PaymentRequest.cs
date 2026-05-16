using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Models.API
{
    /// <summary>
    /// Request for NON-card payment (bank transfer, cash, demo, manual).
    /// </summary>
    public class PaymentRequest
    {
        [Required(ErrorMessage = "Shipment ID is required.")]
        public int ShipmentId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Bank / Cash / Invoice / Demo
        /// </summary>
        [Required(ErrorMessage = "Payment method is required.")]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Used ONLY for manual / cash / office payments
        /// </summary>
        public int? EmployeeId { get; set; }

        // =========================
        // IDENTIFICATION (OPTIONAL)
        // =========================

        public string? PersonalId { get; set; }
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "User type is required.")]
        public UserType UserType { get; set; }
    }
}

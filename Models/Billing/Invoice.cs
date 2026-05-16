using SecureTransparentDataExchange.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Orders;

namespace SecureTransparentDataExchange.Models.Billing
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Number { get; set; } = string.Empty;

        // owner (Login.Id) 
        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Login? User { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0m;
        public int? OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
        [Required, StringLength(10)]
        public string Currency { get; set; } = "EUR";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ so that there is no "0001-01-01" 
        [Required]
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
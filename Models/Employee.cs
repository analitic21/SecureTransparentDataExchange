using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.Billing;
using SecureTransparentDataExchange.Models.Orders;


namespace SecureTransparentDataExchange.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoginId { get; set; }

        [ForeignKey(nameof(LoginId))]
        public Login Login { get; set; } = null!;

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Phone { get; set; } = string.Empty;

        public DateTime? ContractEndDate { get; set; }

        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? TemporaryPassword { get; set; }

        public ICollection<Payment> ProcessedPayments { get; set; } = new List<Payment>();

        public void GenerateTemporaryPassword()
        {
            TemporaryPassword = Guid.NewGuid().ToString("N")[..8];
        }
    }
}
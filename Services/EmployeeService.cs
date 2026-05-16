using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Orders;

namespace SecureTransparentDataExchange.Services
{
    public class EmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ✅ Add
        public async Task<Employee?> AddEmployeeAsync(Employee employee, CancellationToken ct = default)
        {
            if (employee == null) return null;

            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = employee.CreatedAt;

            await _context.Employees.AddAsync(employee, ct);
            await _context.SaveChangesAsync(ct);
            return employee;
        }
        public async Task<List<Employee>> GetAllEmployeesAsync(CancellationToken ct = default)
        {
            return await _context.Employees
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync(ct);
        }
        // ✅ Auto end contracts (Active -> ContractEnded), если срок истёк
        public async Task<int> AutoEndContractEmployeesAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var toEnd = await _context.Employees
                .Where(e => e.Status == EmployeeStatus.Active
                            && e.ContractEndDate != null
                            && e.ContractEndDate <= now)
                .ToListAsync(ct);

            foreach (var e in toEnd)
            {
                e.Status = EmployeeStatus.ContractEnded; // ← используем твой enum
                e.UpdatedAt = now;
            }

            if (toEnd.Count == 0) return 0;

            await _context.SaveChangesAsync(ct);
            return toEnd.Count;
        }

        // ✅ Auto terminate old Pending (> 30 дней)
        public async Task<int> AutoTerminatePendingEmployeesAsync(CancellationToken ct = default)
        {
            var threshold = DateTime.UtcNow.AddDays(-30);

            var toTerminate = await _context.Employees
                .Where(e => e.Status == EmployeeStatus.Pending
                            && e.CreatedAt <= threshold)
                .ToListAsync(ct);

            foreach (var e in toTerminate)
            {
                e.Status = EmployeeStatus.Terminated;
                e.UpdatedAt = DateTime.UtcNow;
            }

            if (toTerminate.Count == 0) return 0;

            await _context.SaveChangesAsync(ct);
            return toTerminate.Count;
        }

        // ✅ Update
        public async Task<bool> UpdateEmployeeAsync(int id, Employee updated, CancellationToken ct = default)
        {
            var e = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (e == null) return false;

            e.Name = updated.Name;
            e.LastName = updated.LastName;
            e.Email = updated.Email;
            e.Phone = updated.Phone;
            e.CompanyId = updated.CompanyId;
            e.ContractEndDate = updated.ContractEndDate;
            e.Status = updated.Status;
            e.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        // ✅ Delete
        public async Task<bool> DeleteEmployeeAsync(int id, CancellationToken ct = default)
        {
            var e = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (e == null) return false;

            _context.Employees.Remove(e);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // ✅ Get by id
        public async Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        // ✅ List by company & type
        public async Task<List<Employee>> GetEmployeesByCompanyIdAndUserTypeAsync(
    int companyId,
    UserType userType,
    CancellationToken ct = default)
        {
            return await _context.Employees
                .Include(e => e.Login) 
                .AsNoTracking()
                .Where(e => e.CompanyId == companyId && e.Login.UserType == userType)
                .ToListAsync(ct);
        }
    }
}

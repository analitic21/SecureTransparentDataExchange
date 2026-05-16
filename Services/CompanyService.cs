using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services
{
    public class CompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(ApplicationDbContext context, ILogger<CompanyService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Method for getting a company by ID
        public async Task<Company?> GetCompanyByIdAsync(int companyId)
        {
            return await _context.Companies.FindAsync(companyId); // Corrected to the correct DbSet - Companies
        }

        // Method for getting all companies
        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies.ToListAsync(); // Corrected to the correct DbSet - Companies
        }

        // Method for updating a company
        public async Task<(bool isSuccess, string message)> UpdateCompanyAsync(int companyId, Company updatedCompany)
        {
            var company = await _context.Companies.FindAsync(companyId); // Corrected to the correct DbSet - Companies

            if (company == null)
            {
                return (false, "Company not found.");
            }

            // Updating company data
            company.CompanyName = updatedCompany.CompanyName;
            company.ContactNumber = updatedCompany.ContactNumber;
            company.Email = updatedCompany.Email;
            company. CompanyAddress= updatedCompany.CompanyAddress;
            company.ContactInfo = updatedCompany.ContactInfo;
            company.RegistrationNumber = updatedCompany.RegistrationNumber;
            company.ContactPerson = updatedCompany.ContactPerson;

            await _context.SaveChangesAsync();

            return (true, "Company updated successfully.");
        }
        public async Task<Company> CreateAsync(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return company;
        }
        // ==========================
        // COMPATIBILITY METHODS
        // ==========================

        public async Task<List<Company>> GetAllAsync()
        {
            return await GetAllCompaniesAsync();
        }

        public async Task<Company?> GetByIdAsync(int id)
        {
            return await GetCompanyByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await DeleteCompanyAsync(id);
            return result.isSuccess;
        }
        // Method for deleting a company 
        public async Task<(bool isSuccess, string message)> DeleteCompanyAsync(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId); // Corrected to the correct DbSet - Companies 

            if (company == null)
            {
                return (false, "Company not found.");
            }

            _context.Companies.Remove(company); // Delete company
            await _context.SaveChangesAsync();

            return (true, "Company deleted successfully.");
        }
    }
}
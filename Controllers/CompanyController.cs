using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly CompanyService _companyService;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(CompanyService companyService, ILogger<CompanyController> logger)
        {
            _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // =============================
        // GET ALL
        // =============================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var companies = await _companyService.GetAllAsync();
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading companies");
                return StatusCode(500, new { message = "Error loading companies" });
            }
        }

        // =============================
        // GET BY ID
        // =============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var company = await _companyService.GetByIdAsync(id);

                if (company == null)
                    return NotFound("Company not found");

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading company");
                return StatusCode(500, new { message = "Error loading company" });
            }
        }

        // =============================
        // CREATE
        // =============================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Company company)
        {
            if (company == null)
                return BadRequest("Invalid data");

            try
            {
                var created = await _companyService.CreateAsync(company);
                return Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                return StatusCode(500, new { message = "Error creating company" });
            }
        }

        // =============================
        // UPDATE
        // =============================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] Company company)
        {
            if (company == null)
                return BadRequest("Invalid data.");

            if (id != company.Id)
                return BadRequest("Company ID mismatch.");

            try
            {
                var result = await _companyService.UpdateCompanyAsync(id, company);

                if (!result.isSuccess)
                    return NotFound(new { message = result.message });

                return Ok(new { message = result.message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company");
                return StatusCode(500, new { message = "Error updating company" });
            }
        }

        // =============================
        // DELETE
        // =============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _companyService.DeleteAsync(id);

                if (!deleted)
                    return NotFound("Company not found");

                return Ok(new { message = "Company deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company");
                return StatusCode(500, new { message = "Error deleting company" });
            }
        }
    }
}
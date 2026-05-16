using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Models.Orders;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        {
            if (employee == null)
                return BadRequest("Employee data is required.");

            var addedEmployee = await _employeeService.AddEmployeeAsync(employee);

            if (addedEmployee == null)
                return BadRequest("Failed to add employee.");

            return CreatedAtAction(nameof(GetEmployeeById), new { id = addedEmployee.Id }, addedEmployee);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee updatedEmployee)
        {
            if (updatedEmployee == null)
                return BadRequest("Employee data is required.");

            if (id != updatedEmployee.Id)
                return BadRequest("Employee ID mismatch.");

            var success = await _employeeService.UpdateEmployeeAsync(id, updatedEmployee);

            if (!success)
                return NotFound($"Employee with ID {id} not found.");

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var success = await _employeeService.DeleteEmployeeAsync(id);

            if (!success)
                return NotFound($"Employee with ID {id} not found.");

            return NoContent();
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound($"Employee with ID {id} not found.");

            return Ok(employee);
        }

        [HttpGet("company/{companyId:int}/type/{userType}")]
        public async Task<IActionResult> GetEmployeesByCompanyAndType(int companyId, UserType userType)
        {
            var employees = await _employeeService.GetEmployeesByCompanyIdAndUserTypeAsync(companyId, userType);

            if (employees == null || employees.Count == 0)
                return NotFound("No employees found for the specified company and user type.");

            return Ok(employees);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }
        [HttpPost("auto-end-contracts")]
        public async Task<IActionResult> AutoEndContractEmployees()
        {
            var count = await _employeeService.AutoEndContractEmployeesAsync();
            return Ok(new { message = $"{count} contract(s) automatically ended." });
        }

        [HttpPost("auto-terminate-pending")]
        public async Task<IActionResult> AutoTerminatePendingEmployees()
        {
            var count = await _employeeService.AutoTerminatePendingEmployeesAsync();
            return Ok(new { message = $"{count} pending employee(s) automatically terminated." });
        }
    }
}
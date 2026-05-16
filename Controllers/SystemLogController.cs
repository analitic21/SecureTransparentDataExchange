using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [Authorize(Roles = "Admin,Administrator")]
    [ApiController]
    [Route("api/[controller]")]
    public class SystemLogController : ControllerBase
    {
        private readonly SystemLogService _systemLogService;
        private readonly ILogger<SystemLogController> _logger;

        public SystemLogController(SystemLogService systemLogService, ILogger<SystemLogController> logger)
        {
            _systemLogService = systemLogService ?? throw new ArgumentNullException(nameof(systemLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Получить все системные логи
        [HttpGet]
        public async Task<ActionResult<List<SystemLog>>> GetSystemLogs()
        {
            try
            {
                var (success, message, logs) = await _systemLogService.GetSystemLogsAsync();
                if (!success || logs == null || logs.Count == 0)
                {
                    _logger.LogWarning(message);
                    return NotFound(new { message });
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving system logs.");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // Получить системный лог по Id
        [HttpGet("{id}")]
        public async Task<ActionResult<SystemLog>> GetSystemLog(int id)
        {
            try
            {
                var (success, message, log) = await _systemLogService.GetSystemLogByIdAsync(id);
                if (!success || log == null)
                {
                    _logger.LogWarning(message);
                    return NotFound(new { message });
                }
                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving system log with id {id}.");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // Добавить новый системный лог
        [HttpPost]
        public async Task<IActionResult> AddSystemLog([FromBody] SystemLog log)
        {
            if (log == null)
            {
                _logger.LogWarning("Received null log data.");
                return BadRequest(new { message = "System log data cannot be null." });
            }

            try
            {
                var (success, message) = await _systemLogService.AddSystemLogAsync(log);
                if (!success)
                {
                    return StatusCode(500, new { message });
                }

                _logger.LogInformation($"New system log with id {log.Id} added successfully.");
                return CreatedAtAction(nameof(GetSystemLog), new { id = log.Id }, log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new system log.");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // Обновить системный лог
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSystemLog(int id, [FromBody] SystemLog log)
        {
            if (log == null || log.Id != id)
            {
                _logger.LogWarning($"Mismatch between log data and ID: {id}");
                return BadRequest(new { message = "Mismatch between log data and Id." });
            }

            try
            {
                var (success, message, existingLog) = await _systemLogService.GetSystemLogByIdAsync(id);
                if (!success || existingLog == null)
                {
                    _logger.LogWarning(message);
                    return NotFound(new { message });
                }

                await _systemLogService.UpdateSystemLogAsync(existingLog);
                _logger.LogInformation($"System log with id {id} updated successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating system log with id {id}.");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // Удалить системный лог
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystemLog(int id)
        {
            try
            {
                var (success, message, existingLog) = await _systemLogService.GetSystemLogByIdAsync(id);
                if (!success || existingLog == null)
                {
                    _logger.LogWarning(message);
                    return NotFound(new { message });
                }

                await _systemLogService.DeleteSystemLogAsync(id);
                _logger.LogInformation($"System log with id {id} deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting system log with id {id}.");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}

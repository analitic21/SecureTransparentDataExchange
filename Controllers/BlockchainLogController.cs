using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models.DTOs;
using SecureTransparentDataExchange.Models.Repositories;
using SecureTransparentDataExchange.Services.Blockchain;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Administrator,Employee")]
    public class BlockchainLogController : ControllerBase
    {
        private readonly BlockchainLogService _blockchainLogService;
        private readonly LedgerFacade _ledger;
        private readonly ILogger<BlockchainLogController> _logger;

        public BlockchainLogController(
            BlockchainLogService blockchainLogService,
            LedgerFacade ledger,
            ILogger<BlockchainLogController> logger)
        {
            _blockchainLogService = blockchainLogService;
            _ledger = ledger;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLogs()
        {
            try
            {
                var logs = (await _blockchainLogService.GetAllLogsAsync())
                    .OrderByDescending(x => x.Id)
                    .ToList();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching blockchain logs.");

                return StatusCode(500, new
                {
                    message = "Internal server error while fetching logs."
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBlockchainLogById(int id)
        {
            try
            {
                var log = await _blockchainLogService.GetBlockchainLogByIdAsync(id);
                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching blockchain log.");

                return StatusCode(500, new
                {
                    message = "Internal server error while fetching blockchain log."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddLog([FromBody] CreateBlockchainLogRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request cannot be null." });

            if (request.ShipmentId <= 0)
                return BadRequest(new { message = "ShipmentId is required." });

            if (string.IsNullOrWhiteSpace(request.Data))
                return BadRequest(new { message = "Data is required." });

            try
            {
                var loginIdValue =
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    User.FindFirstValue("sub") ??
                    User.FindFirstValue("nameid");

                if (!int.TryParse(loginIdValue, out var loginId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid user token."
                    });
                }

                var transactionHash = await _ledger.AppendAsync(
                    request.ShipmentId,
                    loginId,
                    "ManualBlockchainLog",
                    new
                    {
                        data = request.Data,
                        createdBy = loginId,
                        createdAt = DateTime.UtcNow
                    },
                    request.Description ?? "Blockchain log"
                );

                return Ok(new
                {
                    success = true,
                    message = "Blockchain log created successfully.",
                    transactionHash
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blockchain log.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error while creating blockchain log."
                });
            }
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateBlockchain()
        {
            try
            {
                var result = await _blockchainLogService.ValidateChainAsync();

                return Ok(new
                {
                    valid = result.Valid,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating blockchain.");

                return StatusCode(500, new
                {
                    message = "Internal server error while validating blockchain."
                });
            }
        }
    }
}
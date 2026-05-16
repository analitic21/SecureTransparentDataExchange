using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveryHistoryController : ControllerBase
    {
        private readonly DeliveryHistoryService _historyService;

        public DeliveryHistoryController(DeliveryHistoryService historyService)
        {
            _historyService = historyService;
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeliveryHistory>>> GetAllDeliveryHistories()
        {
            var histories = await _historyService.GetAllDeliveryHistoriesAsync();
            return Ok(histories);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DeliveryHistory>> GetDeliveryHistoryById(int id)
        {
            var history = await _historyService.GetDeliveryHistoryByIdAsync(id);

            if (history == null)
                return NotFound(new { message = "Delivery history not found." });

            return Ok(history);
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpPost]
        public async Task<ActionResult<DeliveryHistory>> CreateDeliveryHistory([FromBody] DeliveryHistory history)
        {
            if (history == null)
                return BadRequest(new { message = "Delivery history is required." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdHistory = await _historyService.CreateDeliveryHistoryAsync(history);

            return CreatedAtAction(
                nameof(GetDeliveryHistoryById),
                new { id = createdHistory.Id },
                createdHistory
            );
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<DeliveryHistory>> UpdateDeliveryHistory(int id, [FromBody] DeliveryHistory history)
        {
            if (history == null)
                return BadRequest(new { message = "Delivery history is required." });

            if (id != history.Id)
                return BadRequest(new { message = "ID mismatch." });

            var updatedHistory = await _historyService.UpdateDeliveryHistoryAsync(id, history);

            if (updatedHistory == null)
                return NotFound(new { message = "Delivery history not found." });

            return Ok(updatedHistory);
        }

        [Authorize(Roles = "Admin,Administrator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDeliveryHistory(int id)
        {
            var deleted = await _historyService.DeleteDeliveryHistoryAsync(id);

            if (!deleted)
                return NotFound(new { message = "Delivery history not found." });

            return NoContent();
        }
    }
}
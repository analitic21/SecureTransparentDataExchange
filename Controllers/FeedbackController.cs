using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models.Feedback;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Data;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;

        public FeedbackController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] Feedback feedback)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _feedbackService.SubmitFeedbackAsync(feedback);
            return Ok(new { message = "Thank you for your feedback!" });
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // <-- теперь путь /api/LoginAgreement
    public class LoginAgreementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoginAgreementController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Get the latest agreement (frontend uses this)
        /// GET /api/LoginAgreement/latest
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestAgreement()
        {
            var agreement = await _context.LoginAgreements
                .Include(a => a.Articles)
                .OrderByDescending(a => a.Version)
                .FirstOrDefaultAsync();

            if (agreement == null)
                return NotFound(new { message = "No agreement found" });

            return Ok(agreement);
        }

        /// <summary>
        /// Get all agreements
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.LoginAgreements
                .Include(a => a.Articles)
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Get agreement by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var agreement = await _context.LoginAgreements
                .Include(a => a.Articles)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (agreement == null)
                return NotFound(new { message = "Agreement not found" });

            return Ok(agreement);
        }

        /// <summary>
        /// Create agreement
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LoginAgreement model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid agreement data" });

            model.CreatedAt = DateTime.UtcNow;

            _context.LoginAgreements.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        /// <summary>
        /// Update agreement
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LoginAgreement model)
        {
            var agreement = await _context.LoginAgreements.FindAsync(id);
            if (agreement == null)
                return NotFound(new { message = "Agreement not found" });

            agreement.Version = model.Version;
            agreement.Content = model.Content;

            await _context.SaveChangesAsync();

            return Ok(agreement);
        }

        /// <summary>
        /// Delete agreement
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var agreement = await _context.LoginAgreements.FindAsync(id);
            if (agreement == null)
                return NotFound(new { message = "Agreement not found" });

            _context.LoginAgreements.Remove(agreement);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

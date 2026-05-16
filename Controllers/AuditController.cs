using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Data;
using Microsoft.EntityFrameworkCore;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/audit
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.AuditLogs
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/audit/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _context.AuditLogs.FindAsync(id);
            return item == null ? NotFound() : Ok(item);
        }
    }
}

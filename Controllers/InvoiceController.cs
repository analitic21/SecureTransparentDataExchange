using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models.API.Invoices;
using SecureTransparentDataExchange.Models.Billing;
using SecureTransparentDataExchange.Services;
using SecureTransparentDataExchange.Models.Enums;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly ILogger<InvoiceController> _log;

    public InvoiceController(IInvoiceService service, ILogger<InvoiceController> log)
    {
        _service = service;
        _log = log;
    }

    // 🔥 CREATE (admin only) 
    [Authorize(Roles = "Admin,Administrator")]
    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var inv = await _service.CreateAsync(req, userId);

        return Ok(ToDto(inv));
    }

    // 🔥 CLIENT - only your own 
    [Authorize(Roles = "User,Client,Business")]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> My()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _service.GetForUserAsync(userId);

        return Ok(list.Select(ToDto));
    }

    // 🔥 EMPLOYEE + ADMIN - client invoices 
    [Authorize(Roles = "Employee,Admin,Administrator")]
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> ForUser(int userId)
    {
        var list = await _service.GetForUserAsync(userId);
        return Ok(list.Select(ToDto));
    }

    // 🔥 EMPLOYEE + ADMIN - list 
    [Authorize(Roles = "Employee,Admin,Administrator")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> List(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        var list = await _service.GetPagedAsync(page, pageSize);
        return Ok(list.Select(ToDto));
    }

    // 🔥 UPDATE - admin only 
    [Authorize(Roles = "Admin,Administrator")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateInvoiceRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var ok = await _service.UpdateAsync(req);
        return ok ? Ok() : NotFound();
    }

    // 🔥 EMPLOYEE can mark paid 
    [Authorize(Roles = "Employee,Admin,Administrator")]
    [HttpPost("{id:int}/paid")]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var ok = await _service.MarkPaidAsync(id);
        return ok ? Ok() : NotFound();
    }

    // 🔥 only admin can cancel 
    [Authorize(Roles = "Admin,Administrator")]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var ok = await _service.CancelAsync(id);
        return ok ? Ok() : NotFound();
    }

    private static InvoiceDto ToDto(SecureTransparentDataExchange.Models.Billing.Invoice i)
    {
        return new InvoiceDto
        {
            Id = i.Id,
            Number = i.Number,
            Amount = i.Amount,
            Status = i.Status,
            CreatedAt = i.CreatedAt,
            DueDate = i.DueDate,
            UserId = i.UserId,
            Description = i.Description
        };
    }
}
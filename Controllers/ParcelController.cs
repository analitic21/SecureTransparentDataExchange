using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTransparentDataExchange.Data;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models;


namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParcelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ParcelController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Administrator,Employee")]
[HttpPost]
public async Task<ActionResult<ParcelDto>> CreateParcel([FromBody] CreateParcelRequest request)
{
    if (request == null)
        return BadRequest(new { message = "Parcel payload is required." });

    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    try
    {
        var parcel = new Parcel
        {
            TrackingNumber = request.TrackingNumber ?? string.Empty,
            Sender = request.Sender ?? string.Empty,
            Receiver = request.Receiver ?? string.Empty,
            Status = request.Status ?? string.Empty,
            CargoTypeId = request.CargoTypeId,
            ShipmentId = request.ShipmentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();

        return Ok(new ParcelDto
        {
            Id = parcel.Id,
            TrackingNumber = parcel.TrackingNumber,
            Sender = parcel.Sender,
            Receiver = parcel.Receiver,
            Status = parcel.Status,
            CargoTypeId = parcel.CargoTypeId,
            ShipmentId = parcel.ShipmentId,
            CreatedAt = parcel.CreatedAt
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            message = "Error creating parcel.",
            details = ex.Message
        });
    }
}


        // Logged-in users can get parcel by ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ParcelDto>> GetParcel(int id)
        {
            try
            {
                var parcel = await _context.Parcels
                    .Include(p => p.CargoType)
                    .Include(p => p.Shipment)
                    .Include(p => p.IoTDevice)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (parcel == null)
                    return NotFound(new { message = "Parcel not found" });

                return Ok(new ParcelDto
                {
                    Id = parcel.Id,
                    TrackingNumber = parcel.TrackingNumber,
                    Sender = parcel.Sender,
                    Receiver = parcel.Receiver,
                    Status = parcel.Status,
                    CargoTypeId = parcel.CargoTypeId,
                    ShipmentId = parcel.ShipmentId,
                    CreatedAt = parcel.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error loading parcel.",
                    details = ex.Message
                });
            }
        }
        // Admin/Employee only
        [Authorize(Roles = "Admin,Administrator,Employee")]
        [HttpGet]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ParcelDto>>> GetAllParcels()
        {
            try
            {
                var parcels = await _context.Parcels
                    .Select(p => new ParcelDto
                    {
                        Id = p.Id,
                        TrackingNumber = p.TrackingNumber,
                        Sender = p.Sender,
                        Receiver = p.Receiver,
                        Status = p.Status,
                        CargoTypeId = p.CargoTypeId,
                        ShipmentId = p.ShipmentId,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                return Ok(parcels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error loading parcels.",
                    details = ex.Message
                });
            }
        }



        // Admin only
        [Authorize(Roles = "Admin,Administrator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteParcel(int id)
        {
            try
            {
                var parcel = await _context.Parcels.FindAsync(id);
                if (parcel == null)
                    return NotFound(new { message = "Parcel not found" });

                _context.Parcels.Remove(parcel);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Parcel deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error deleting parcel.",
                    details = ex.Message
                });
            }
        }
    }
}
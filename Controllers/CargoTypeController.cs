using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CargoTypeController : ControllerBase
    {
        private readonly CargoTypeService _cargoTypeService;

        public CargoTypeController(CargoTypeService cargoTypeService)
        {
            _cargoTypeService = cargoTypeService;
        }

        // GET api/CargoType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CargoType>>> GetAll()
        {
            return Ok(await _cargoTypeService.GetAllCargoTypesAsync());
        }

        // GET api/CargoType/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CargoType>> GetById(int id)
        {
            var ct = await _cargoTypeService.GetCargoTypeAsync(id);
            if (ct == null) return NotFound();
            return Ok(ct);
        }

        // POST api/CargoType
        [HttpPost]
        public async Task<ActionResult<CargoType>> Create([FromBody] CargoType model)
        {
            var created = await _cargoTypeService.AddCargoTypeAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/CargoType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CargoType model)
        {
            if (model.Id != id)
                return BadRequest("ID mismatch");

            var exists = await _cargoTypeService.GetCargoTypeAsync(id);
            if (exists == null)
                return NotFound();

            await _cargoTypeService.UpdateCargoTypeAsync(model);
            return NoContent();
        }

        // DELETE api/CargoType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _cargoTypeService.DeleteCargoTypeAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}

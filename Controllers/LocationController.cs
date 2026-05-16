using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using  SecureTransparentDataExchange.DTOs;
using SecureTransparentDataExchange.Models;              
using SecureTransparentDataExchange.Models.Location;    
using SecureTransparentDataExchange.Services;


namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;

        public LocationController(LocationService locationService)
        {
            _locationService = locationService;
        }

        // Get all countries 
        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<CountryDTO>>> GetCountries()
        {
            var countries = await _locationService.GetAllCountriesAsync();
            return Ok(countries);
        }

        // Get all cities
        [HttpGet("cities")]
        public async Task<ActionResult<IEnumerable<CityDTO>>> GetCities()
        {
            var cities = await _locationService.GetAllCitiesAsync();
            return Ok(cities);
        }

        // Get all postal codes
        [HttpGet("postal-codes")]
        public async Task<ActionResult<IEnumerable<PostalCodeDTO>>> GetPostalCodes()
        {
            var postalCodes = await _locationService.GetAllPostalCodesAsync();
            return Ok(postalCodes);
        }

        // Add country 
        [HttpPost("add-country")]
        public async Task<IActionResult> AddCountry([FromBody] CountryDTO countryDTO)
        {
            if (countryDTO == null)
                return BadRequest("Invalid country data");

            var country = new Country
            {
                Name = countryDTO.Name
            };

            await _locationService.AddCountryAsync(country);
            return CreatedAtAction(nameof(GetCountries), new { id = country.Id }, country);
        }

        // Add city 
        [HttpPost("add-city")]
        public async Task<IActionResult> AddCity([FromBody] CityDTO cityDTO)
        {
            if (cityDTO == null)
                return BadRequest("Invalid city data");

            var city = new City
            {
                Name = cityDTO.Name,
                CountryId = cityDTO.CountryId
            };

            await _locationService.AddCityAsync(city);
            return CreatedAtAction(nameof(GetCities), new { id = city.Id }, city);
        }

        // Add postal code
        [HttpPost("add-postal-code")]
        public async Task<IActionResult> AddPostalCode([FromBody] PostalCodeDTO postalCodeDTO)
        {
            if (postalCodeDTO == null)
                return BadRequest("Invalid postal code data");

            var postalCode = new PostalCode
            {
                Code = postalCodeDTO.Code
            };

            await _locationService.AddPostalCodeAsync(postalCode);
            return CreatedAtAction(nameof(GetPostalCodes), new { id = postalCode.Id }, postalCode);
        }
    }
}

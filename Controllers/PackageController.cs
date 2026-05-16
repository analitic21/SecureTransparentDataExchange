using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models.API;
using SecureTransparentDataExchange.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Employee,Admin,Administrator")]
public class PackageController : ControllerBase
{
    private readonly PackageService _packageService;

    public PackageController(PackageService packageService)
    {
        _packageService = packageService;
    }

    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] PackageScanRequest request)
    {
        if (request == null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.TrackingNumber))
            return BadRequest("Tracking number is required.");

        var package = await _packageService.RegisterOrUpdatePackageAsync(
            request.TrackingNumber.Trim()
        );

        return Ok(package);
    }
}
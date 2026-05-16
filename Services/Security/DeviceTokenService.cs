using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureTransparentDataExchange.Services.Security;

public class DeviceTokenService
{
    private readonly IConfiguration _cfg;

    public DeviceTokenService(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    public string GenerateDeviceToken(int deviceId, string trackingNumber)
    {
        var key = _cfg["Jwt:Key"] ?? throw new Exception("Jwt:Key missing");
        var issuer = _cfg["Jwt:Issuer"];
        var audience = _cfg["Jwt:Audience"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("type", "iot-device"),
            new Claim("tracking", trackingNumber),
            new Claim("deviceId", deviceId.ToString()),
            new Claim(ClaimTypes.Role, "device")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddYears(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
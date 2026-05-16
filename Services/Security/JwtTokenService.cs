using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SecureTransparentDataExchange.Models.identity;
using SecureTransparentDataExchange.Models.API.Auth;


namespace SecureTransparentDataExchange.Services.Security
{
    public class JwtTokenService
    {
        private readonly JwtSettingService _jwtSettingService;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(
            JwtSettingService jwtSettingService,
            ILogger<JwtTokenService> logger)
        {
            _jwtSettingService = jwtSettingService;
            _logger = logger;
        }

        public async Task<JwtTokenResult> GenerateJwtAsync(Login user)
        {
            var jwt = await _jwtSettingService.GetOrCreateJwtConfigAsync();

            var role = user.Role?.Name ?? "User";

            var claims = new[]
            {
    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email ?? ""),
    new Claim(ClaimTypes.Name, user.UserName ?? ""),

    new Claim(ClaimTypes.Role, role),
    new Claim("role", role)
};

            var key = new SymmetricSecurityKey(
                Convert.FromBase64String(jwt.SecretKey)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwt.AccessTokenLifetimeMinutes),
                signingCredentials: creds
            );

            return new JwtTokenResult(
                true,
                "JWT generated",
                new JwtSecurityTokenHandler().WriteToken(token)
            );
        }
    }
}

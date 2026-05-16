using System;

namespace SecureTransparentDataExchange.Models.Security
{
    public class JwtRuntimeConfig
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        // ✅ ACCESS TOKEN lifetime (in minutes) 
        public int AccessTokenLifetimeMinutes { get; set; } = 120;
    }
}
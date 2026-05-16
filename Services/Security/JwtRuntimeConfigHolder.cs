namespace SecureTransparentDataExchange.Services.Security
{
    /// <summary>
    /// Holds active JWT config in memory (loaded once from DB at app start).
    /// </summary>
    public class JwtRuntimeConfigHolder
    {
        public string SecretKeyBase64 { get; set; } = string.Empty; // base64 string
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}

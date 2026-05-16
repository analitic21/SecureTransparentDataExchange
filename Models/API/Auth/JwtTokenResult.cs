namespace SecureTransparentDataExchange.Models.API.Auth
{
    public class JwtTokenResult
    {
        public bool Success { get; }
        public string Message { get; }
        public string Token { get; }

        public JwtTokenResult(bool success, string message, string token)
        {
            Success = success;
            Message = message;
            Token = token;
        }
    }
}

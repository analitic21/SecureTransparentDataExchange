namespace SecureTransparentDataExchange.Models.API.Auth
{
    public class RefreshTokenResult
    {
        public bool Success { get; }
        public string Message { get; }
        public string? Token { get; }

        public RefreshTokenResult(bool success, string message, string? token)
        {
            Success = success;
            Message = message;
            Token = token;
        }
    }
}

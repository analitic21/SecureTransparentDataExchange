namespace SecureTransparentDataExchange.Options
{
    public class SmsSettings
    {
        public string? BaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? Sender { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }
}

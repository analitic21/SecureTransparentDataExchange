namespace SecureTransparentDataExchange.Options
{
    /// <summary>
    /// Stripe configuration options loaded from appsettings.json
    /// </summary>
    public sealed class StripeOptions
    {
        /// <summary>
        /// Stripe Secret API Key (sk_test_...)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Stripe Webhook signing secret (whsec_...)
        /// </summary>
        public string WebhookSecret { get; set; } = string.Empty;
    }
}

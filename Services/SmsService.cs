using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SecureTransparentDataExchange.Services
{
    public class SmsService
    {
        private readonly SmsSettings _settings;
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;

        public SmsService(
            IOptions<SmsSettings> options,
            ILogger<SmsService> logger,
            HttpClient httpClient)
        {
            _settings = options.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task SendSmsAsync(SmsModel sms)
        {
            ArgumentNullException.ThrowIfNull(sms);

            if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
                string.IsNullOrWhiteSpace(_settings.Sender) ||
                string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                throw new InvalidOperationException("SMS configuration is incomplete.");
            }

            var message = sms.BuildMessage();

            var requestBody = new
            {
                from = _settings.Sender,
                to = new[] { sms.PhoneNumber },
                body = message
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl}/batches")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("SMS failed: {Status} {Body}", response.StatusCode, body);
                throw new InvalidOperationException("SMS provider error");
            }

            _logger.LogInformation("SMS sent to {Phone}", sms.PhoneNumber);
        }
    }
}
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SecureTransparentDataExchange.Auth
{
    public class AIAuthenticationService
    {
        private readonly ILogger<AIAuthenticationService> _logger;
        private readonly HashSet<string> _trustedIPAddresses = new HashSet<string>
 {
 "192.168.1.1", // Example of a trusted IP
 "203.0.113.0", // Example of a trusted IP
 };

        public AIAuthenticationService(ILogger<AIAuthenticationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Analyze login attempts and check for suspicious activity.
        /// </summary>
        public bool AnalyzeLoginAttempt(string email, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(ipAddress))
            {
                _logger.LogWarning("Login attempt detected with missing email or IP address.");
                return true; // Suspicious attempt by default
            }

            bool isSuspicious = !_trustedIPAddresses.Contains(ipAddress);

            if (isSuspicious)
            {
                _logger.LogWarning($"⚠️ Suspicious login attempt detected for {MaskEmail(email)} from IP: {ipAddress}");
            }

            return isSuspicious;
        }

        /// <summary>
        /// Analyze the behavioral pattern of logins based on frequency or unusual activity.
        /// </summary>
        public bool AnalyzeBehavioralPattern(string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                _logger.LogWarning("Behavioral analysis failed: Missing user email.");
                return true; // Suspicious activity by default
            }

            int loginFrequency = GetLoginFrequency(userEmail);
            bool isAbnormalPattern = loginFrequency > 10;

            if (isAbnormalPattern)
            {
                _logger.LogWarning($"⚠️ Abnormal login pattern detected for {MaskEmail(userEmail)}. Frequency: {loginFrequency}");
            }

            return isAbnormalPattern;
        }

        /// <summary>
        /// Get the user's login frequency from the database (replaced random selection with sample code).
        /// </summary>
        private int GetLoginFrequency(string email)
        {
            // Sample logic for accessing the database
            var recentLogins = new List<DateTime>
              {
                DateTime.UtcNow.AddMinutes(-30),
                DateTime.UtcNow.AddMinutes(-20),
                DateTime.UtcNow.AddMinutes(-10)

                };

            return recentLogins.Count;
        }

        /// <summary>
        /// Masks the email to protect sensitive data in logs.
        /// </summary>
        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length == 2)
            {
                return $"{parts[0].Substring(0, 2)}***@{parts[1]}";
            }

            return "Invalid email";
        }
    }
}
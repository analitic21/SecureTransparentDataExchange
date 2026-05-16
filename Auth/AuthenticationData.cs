using System;
using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Auth
{
    public class AuthenticationData
    {
        /// <summary>
        /// Time of day when the event occurred (represented in minutes since midnight).
        /// Example: 0 = 12:00 AM, 720 = 12:00 PM.
        /// </summary>
        [Range(0, 1440, ErrorMessage = "TimeOfDay must be between 0 and 1440 minutes (24 hours).")]
        public int TimeOfDay { get; set; }

        /// <summary>
        /// Number of failed user login attempts.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "FailedAttempts cannot be negative.")]
        public int FailedAttempts { get; set; }

        /// <summary>
        /// Distance between IP addresses in kilometers (for anomaly detection).
        /// </summary>
        [Range(0.0, float.MaxValue, ErrorMessage = "IPAddressDistance must be non-negative.")]
        public float IPAddressDistance { get; set; }

        /// <summary>
        /// Method to convert `TimeOfDay` into human-readable format.
        /// </summary>
        public string GetFormattedTime()
        {
            TimeSpan time = TimeSpan.FromMinutes(TimeOfDay);
            return time.ToString(@"hh\:mm");
        }

        /// <summary>
        /// For debugging and analysis
        /// </summary>
        public override string ToString()
        {
            return $"TimeOfDay: {GetFormattedTime()}, FailedAttempts: {FailedAttempts}, IP Distance: {IPAddressDistance} km";
        }
    }
}
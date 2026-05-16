using System;
using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class NotificationModel
    {
        [Required(ErrorMessage = "Notification type is required.")]
        public string NotificationType { get; set; } = "info"; // info, warning, error, success 

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = "System Notification";

        [Required(ErrorMessage = "Message is required.")]
        public string Message { get; set; } = "Default message";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Optional user ID (if you want to send to a specific one) 
        public string? UserId { get; set; }
    }
}
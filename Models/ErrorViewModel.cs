namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// View model for displaying error information.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Unique identifier of the request associated with the error.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Specifies whether to display the request identifier.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Error message (optional).
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Error code (optional).
        /// </summary>
        public int? ErrorCode { get; set; }
    }
}
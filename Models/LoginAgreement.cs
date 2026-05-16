using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Represents a user agreement with versioning.
    /// </summary>
    public class LoginAgreement
    {
        /// <summary>
        /// Primary key (ID of the agreement).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Title of the agreement.
        /// </summary>
        [Required]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Version of the agreement.
        /// </summary>
        [Required]
        [StringLength(10, ErrorMessage = "Version string cannot exceed 10 characters.")]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Agreement text (legal content).
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the agreement was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates if the agreement is the latest version.
        /// </summary>
        public bool IsLatest { get; set; } = true;

        /// <summary>
        /// Navigation property to related LoginAgreementConsents
        /// </summary>
        public virtual ICollection<LoginAgreementConsent> LoginAgreementConsents { get; set; } = new List<LoginAgreementConsent>();

        /// <summary>
        /// Navigation property to related LoginAgreementArticles
        /// </summary>
        public virtual ICollection<LoginAgreementArticle> Articles { get; set; } = new List<LoginAgreementArticle>();  // Добавлено свойство для связи с статьями

        /// <summary>
        /// Constructor to initialize agreement version and creation date.
        /// </summary>
        public LoginAgreement()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}

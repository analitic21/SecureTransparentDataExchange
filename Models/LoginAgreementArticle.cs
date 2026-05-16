using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Represents an article of a user agreement.
    /// </summary>
    public class LoginAgreementArticle
    {
        /// <summary>
        /// Primary key of the article.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key for the associated agreement.
        /// </summary>
        [Required]
        public int AgreementId { get; set; }

        /// <summary>
        /// Navigation property to the associated agreement.
        /// </summary>
        [ForeignKey(nameof(AgreementId))]
        public virtual LoginAgreement Agreement { get; set; } = null!;

        /// <summary>
        /// Title of the article (e.g., "Article 5").
        /// </summary>
        [Required]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Content of the article (text).
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the article was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

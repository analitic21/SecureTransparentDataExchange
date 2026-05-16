using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Represents a request model for deleting a user.
    /// </summary>
    public class DeleteUserModel
    {
        /// <summary>
        /// Identifier of the user to be deleted.
        /// </summary>
        [Required(ErrorMessage = "User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int Id { get; set; }

        // Additionally, you can add checks for user relationships with other entities,
        // to prevent accidental deletion of a user if this violates the logic of business processes.
    }
}
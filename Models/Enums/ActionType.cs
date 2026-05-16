using SecureTransparentDataExchange.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models.Enums
{
    /// <summary>
    /// Action types for transactions.
    /// </summary>
    public enum ActionType
    {
        Created,
        Updated,
        Deleted
    }
}

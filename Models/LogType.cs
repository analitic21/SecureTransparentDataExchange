using SecureTransparentDataExchange.Models; // For LogType
// Additional standard using for other functions
using System; // Common C# classes
using System.Collections.Generic; // For collections
using System.Linq; // For LINQ
using System.Threading.Tasks; // For asynchronous operations

namespace SecureTransparentDataExchange.Models
{
    /// <summary>
    /// Log types for auditing (info, warning, error).
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Information.
        /// </summary>
        Info,

        /// <summary>
        /// Warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Error.
        /// </summary>
        Error
    }
}
using System; // For basic data types
using System.Collections.Generic; // For collections, if needed
using System.Linq; // For LINQ queries, if needed

namespace SecureTransparentDataExchange.Models
{
    // Generic class for input data
    public class InputData
    {
        // Features to analyze
        public float Feature1 { get; set; }
        public float Feature2 { get; set; }
        public float Feature3 { get; set; }

        // Other features that can be used for processing in the model
        public float Feature4 { get; set; }
        public float Feature5 { get; set; }

        // Additional properties can be added as needed
    }
}
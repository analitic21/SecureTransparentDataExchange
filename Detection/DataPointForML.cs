using System;
using Microsoft.ML.Data;

namespace SecureTransparentDataExchange.Models
{
    public class DataPointForML
    {
        [VectorType(3)]
        public float[] Features { get; set; } = Array.Empty<float>();

        public DateTime Timestamp { get; set; }
    }
}

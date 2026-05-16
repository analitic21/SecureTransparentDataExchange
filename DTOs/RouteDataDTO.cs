using System.Collections.Generic;

namespace SecureTransparentDataExchange.DTOs
{
    /// <summary>
    /// Route data: square distance matrix between points.
    /// </summary>
    public class RouteDataDTO
    {
        /// <summary>
        /// Distance matrix [i][j] between point i and point j.
        /// Must be square: Distances.Count == Distances[i].Count.
        /// </summary>
        public List<List<int>> Distances { get; set; } = new();
    }
}

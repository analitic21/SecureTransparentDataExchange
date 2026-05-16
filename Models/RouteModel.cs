using System;
using System.Collections.Generic;

namespace SecureTransparentDataExchange.Models
{
    public class RouteModel
    {
        // Optimized route (List of points in order)
        public List<int> Route { get; set; } = new List<int>();

        // Total distance of the optimized route
        public double TotalDistance { get; set; }
    }
}

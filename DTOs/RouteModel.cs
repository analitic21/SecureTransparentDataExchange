using System.Collections.Generic;

namespace SecureTransparentDataExchange.DTOs
{
    public class RouteModel
    {
        public List<int> Route { get; set; } = new();
        public double TotalDistance { get; set; }
    }
}

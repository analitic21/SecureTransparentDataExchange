using System.Collections.Generic;

namespace SecureTransparentDataExchange.DTOs
{
    public class LocationTreeDTO
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;

        public List<CityNodeDTO> Cities { get; set; } = new();
    }
}

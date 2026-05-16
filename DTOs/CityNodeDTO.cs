using System.Collections.Generic;

namespace SecureTransparentDataExchange.DTOs
{
    public class CityNodeDTO
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;

        public List<PostalNodeDTO> PostalCodes { get; set; } = new();
    }
}

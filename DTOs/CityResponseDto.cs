using System;

namespace SecureTransparentDataExchange.DTOs
{
    /// <summary>City + country + street + postal code</summary> 
    public class CityResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}
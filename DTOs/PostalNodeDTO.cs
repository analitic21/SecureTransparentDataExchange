using  SecureTransparentDataExchange.DTOs;
using System.Collections.Generic;

namespace SecureTransparentDataExchange.DTOs
{
    public class PostalNodeDTO
    {
        public int PostalCodeId { get; set; }
        public string Code { get; set; } = string.Empty;

        public List<AddressNodeDTO> Addresses { get; set; } = new();
    }
}

using System;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Services.Interfaces;

namespace SecureTransparentDataExchange.Services
{
    public class OrderNumberService : IOrderNumberService
    {
        public string Generate(UserType userType, string? source = null)
        {
            var prefix = userType == UserType.LegalEntity ? "B2B" : "B2C";
            var src = string.IsNullOrWhiteSpace(source) ? "WEB" : source.ToUpper();

            return $"{prefix}-{src}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N[..6]}";
        }
    }
}

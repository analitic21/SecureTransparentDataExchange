using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Services.Interfaces
{
    public interface IOrderNumberService
    {
        string Generate(
            UserType userType,
            string? source = null
        );
    }
}

using SecureTransparentDataExchange.Models.Enums;

namespace SecureTransparentDataExchange.Models.Requests;

public class ChangeLoginTypeRequest
{
    public UserType UserType { get; set; }
}
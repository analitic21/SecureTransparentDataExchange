using System;
using System.ComponentModel;
using System.Reflection;

namespace SecureTransparentDataExchange.Extensions
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            // Get the Description attribute for the enumeration
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();

            // Return the description if the attribute is found, or a string value
            return attribute?.Description ?? value.ToString();
        }
    }
}
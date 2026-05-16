using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.DTOs
{
	public class CityDTO
	{
		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public int CountryId { get; set; }
	}
}
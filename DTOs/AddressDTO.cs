using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.DTOs
{
	public class AddressDTO
	{
		public int Id { get; set; }

		[Required]
		[StringLength(255)]
		public string Street { get; set; } = string.Empty;

		public int PostalCodeId { get; set; }

		public string? PostalCode { get; set; }

		public int? CityId { get; set; }

		public string? City { get; set; }

		public int? CountryId { get; set; }

		public string? Country { get; set; }

		public double? Latitude { get; set; }

		public double? Longitude { get; set; }
	}
}
using System.ComponentModel.DataAnnotations;

public class CityCreateDTO
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Required]
    public int CountryId { get; set; }
}

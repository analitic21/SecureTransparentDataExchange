using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using SecureTransparentDataExchange.Models.Location;

namespace SecureTransparentDataExchange.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [JsonIgnore]
        public virtual ICollection<City> Cities { get; set; } = new List<City>();
    }
}
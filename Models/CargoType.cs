using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecureTransparentDataExchange.Models
{
    public class CargoType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // Cargo type name

        // Parcel association (one cargo type can be associated with multiple parcels)
        public virtual ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();
    }
}
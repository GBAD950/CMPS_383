using System.ComponentModel.DataAnnotations;

namespace Products_383.Data.Models
{
    public class SaleEvent
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a name")]
        [MaxLength(120)]
        public string? Name { get; set; }
        public DateTimeOffset StartUtc { get; set; }
        public DateTimeOffset EndUtc { get; set; }

        public ICollection<SaleEventProduct> Products { get; set; }
    }
}

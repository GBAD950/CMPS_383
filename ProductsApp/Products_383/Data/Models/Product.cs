using System.ComponentModel.DataAnnotations;

namespace Products_383.Data.Models
{
    public class Product
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a name")]
        [MaxLength(120)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Please enter a Description")]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a price greater than {0}")]
        public decimal Price { get; set; }

        public ICollection<SaleEventProduct> SaleEventProducts { get; set; }
    }
}

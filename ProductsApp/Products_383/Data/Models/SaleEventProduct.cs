using System.ComponentModel.DataAnnotations;

namespace Products_383.Data.Models
{
    public class SaleEventProduct
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please enter a price greater than {0}")]
        public decimal SaleEventPrice { get; set; }
        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        public int SaleEventID { get; set; }
        public virtual SaleEvent SaleEvents { get; set; }
    }
}

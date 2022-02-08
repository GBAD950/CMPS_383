using System.ComponentModel.DataAnnotations;

namespace SP22.P03.Web.Features.Products;

public class ProductDto
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
    [Range(1, int.MaxValue, ErrorMessage = "Please enter a price greater than {1}")]
    public decimal Price { get; set; }

    public decimal? SalePrice { get; set; }

    public DateTimeOffset? SaleEndUtc { get; set; }
}
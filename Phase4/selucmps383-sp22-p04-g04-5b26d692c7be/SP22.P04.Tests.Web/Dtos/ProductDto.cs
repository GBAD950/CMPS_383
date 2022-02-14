using System;

namespace SP22.P04.Tests.Web.Dtos;

internal class ProductDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }

    public decimal? SalePrice { get; set; }
    public DateTimeOffset? SaleEndUtc { get; set; }
}
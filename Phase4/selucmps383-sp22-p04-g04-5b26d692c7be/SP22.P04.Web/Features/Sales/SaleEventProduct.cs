using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP22.P04.Web.Features.Products;

namespace SP22.P04.Web.Features.Sales;

public class SaleEventProduct
{
    public int Id { get; set; }
    public decimal SaleEventPrice { get; set; }

    public int ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public int SaleEventId { get; set; }
    public virtual SaleEvent? SaleEvent { get; set; }
}

public class SaleEventProductConfiguration : IEntityTypeConfiguration<SaleEventProduct>
{
    public void Configure(EntityTypeBuilder<SaleEventProduct> builder)
    {
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SP22.P04.Web.Features.Sales;

public class SaleEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }

    public virtual ICollection<SaleEventProduct> Products { get; set; } = new List<SaleEventProduct>();
}

public class SaleEventConfiguration : IEntityTypeConfiguration<SaleEvent>
{
    public void Configure(EntityTypeBuilder<SaleEvent> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);
    }
}
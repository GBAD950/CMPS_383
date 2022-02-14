using Microsoft.EntityFrameworkCore;
using SP22.P04.Web.Features.Products;
using SP22.P04.Web.Features.Sales;

namespace SP22.P04.Web.Data;

public static class MigrateAndSeed
{
    public static void Initialize(IServiceProvider services)
    {
        var context = services.GetRequiredService<DataContext>();
        context.Database.Migrate();

        AddProducts(context);
        AddSaleEvent(context);
    }

    private static void AddSaleEvent(DataContext context)
    {
        var saleEvents = context.Set<SaleEvent>();
        if (saleEvents.Any())
        {
            return;
        }
        var products = context.Set<Product>();
        var product = products.First();
        var saleEvent = new SaleEvent
        {
            Name = "Spring Sale",
            StartUtc = DateTimeOffset.UtcNow,
            EndUtc = DateTimeOffset.UtcNow.AddDays(1),
            Products = 
            {
                new SaleEventProduct
                {
                    Product = product,
                    SaleEventPrice = 0,
                }
            }
        };
        saleEvents.Add(saleEvent);
        context.SaveChanges();
    }

    private static void AddProducts(DataContext context)
    {
        var products = context.Set<Product>();
        if (products.Any())
        {
            return;
        }

        products.Add(new Product
        {
            Name = "Half Life 2",
            Description = "A game",
            Price = 12.99m,
        });
        products.Add(new Product
        {
            Name = "Visual Studio 2022: Professional",
            Description = "A program",
            Price = 199m,
        });
        products.Add(new Product
        {
            Name = "Photoshop",
            Description = "Fancy mspaint",
            Price = 10m,
        });
        context.SaveChanges();
    }
}
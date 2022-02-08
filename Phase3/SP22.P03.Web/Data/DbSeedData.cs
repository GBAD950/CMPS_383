using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SP22.P03.Web.Data.Models;
using System;
using System.Linq;

namespace SP22.P03.Web.Data
{
    public static class DbSeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new DataContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<DataContext>>()))
            {

                // FIX THIS --> It does not add all entries to DB
                // It stops after first condition is met.
                // Re think the consitional statements

                if (!context.Products.Any())
                {
                    context.Products.AddRange(
                   new Product
                   {
                       Name = "Something Funny",
                       Description = "This is a funny product",
                       Price = 35.25M
                   },

                   new Product
                   {
                       Name = "Something Kinda Funny",
                       Description = "This is a slightly funny product",
                       Price = 45M
                   },

                   new Product
                   {
                       Name = "Something LAME",
                       Description = "This is a really LAME product, Please dont buy",
                       Price = 25M
                   },

                   new Product
                   {
                       Name = "Great Product",
                       Description = "This is a really GREAT product, Please buy me",
                       Price = 11M
                   }
                );

                }


                else if (!context.SaleEvents.Any())
                {
                    context.SaleEvents.AddRange(
                     new SaleEvent
                     {
                         Name = "Yoo Hooo",
                         StartUtc = DateTimeOffset.UtcNow,
                         EndUtc = DateTimeOffset.UtcNow.AddDays(2).AddHours(1)

                     },

                     new SaleEvent
                     {
                         Name = "Big Summer Blowout!",
                         StartUtc = DateTimeOffset.UtcNow.AddDays(5),
                         EndUtc = DateTimeOffset.UtcNow.AddDays(7).AddHours(5)

                     }
                    );
                }
                             
               else if (!context.SaleEventsProduct.Any())
                {
                    context.SaleEventsProduct.AddRange(
                   new SaleEventProduct
                   {
                       SaleEventPrice = 21.5M,
                       ProductId = 1,
                       SaleEventID = 1
                   },

                   new SaleEventProduct
                   {
                       SaleEventPrice = 25.5M,
                       ProductId = 2,
                       SaleEventID = 1
                   },

                   new SaleEventProduct
                   {
                       SaleEventPrice = 11.5M,
                       ProductId = 3,
                       SaleEventID = 2
                   }
                    );

                    context.SaveChanges();
                }

                else return;
               
            }
        }
    }
}
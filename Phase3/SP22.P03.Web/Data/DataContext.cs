using Microsoft.EntityFrameworkCore;
using SP22.P03.Web.Data.Models;

namespace SP22.P03.Web.Data
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<SaleEvent> SaleEvents { get; set; }

        public DbSet<SaleEventProduct> SaleEventsProduct { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<SaleEvent>().ToTable("SaleEvent");
            modelBuilder.Entity<SaleEventProduct>().ToTable("SaleEventProduct");
        }


    }
}

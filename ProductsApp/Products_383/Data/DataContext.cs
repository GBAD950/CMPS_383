using Microsoft.EntityFrameworkCore;
using Products_383.Data.Models;

namespace Products_383.Data
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

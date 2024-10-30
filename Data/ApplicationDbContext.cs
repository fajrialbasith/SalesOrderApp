using Microsoft.EntityFrameworkCore;
using SalesOrderApp.Models;

namespace SalesOrderApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SalesOrder>().ToTable("SO_ORDER");
            modelBuilder.Entity<Customer>().ToTable("COM_CUSTOMER");
            modelBuilder.Entity<Item>().ToTable("SO_ITEM");
        }
    }
}

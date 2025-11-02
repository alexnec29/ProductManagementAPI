using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Features.Products;

namespace ProductManagementAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } // This is your Products table
    }
}
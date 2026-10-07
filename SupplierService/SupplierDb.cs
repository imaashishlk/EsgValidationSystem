using Microsoft.EntityFrameworkCore;

namespace SupplierService
{
    public class Supplier
    {
        public int Id { get; set; }
        public string OrgNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IndustryCode { get; set; } = string.Empty;
    }

    public class SupplierDbContext : DbContext
    {
        public SupplierDbContext(DbContextOptions<SupplierDbContext> options) : base(options)
        {
        }
        public DbSet<Supplier> Suppliers { get; set; } = null!;
    }
}

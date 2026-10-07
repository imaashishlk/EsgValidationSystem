using Microsoft.EntityFrameworkCore;
using System;

namespace ComplianceService
{
    public class ComplianceReport
    {
        public int Id { get; set; }
        // We only store the ID. No Foreign Key relationship to the Supplier class.
        // We are dealing with a microservices architecture, so we don't want to create a tight coupling between services.
        public int SupplierId { get; set; }
        public int EsgScore { get; set; }
        public bool IsHighRisk { get; set; }
        public DateTime AssessedDate { get; set; }
    }

    public class ComplianceDbContext : DbContext
    {
        public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : base(options) { }
        public DbSet<ComplianceReport> ComplianceReports => Set<ComplianceReport>();
    }
}

using ComplianceService;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Registering PostgreSQL context
builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// GET reports for a specific supplier
app.MapGet("/api/compliance/{supplierId}", async (int supplierId, ComplianceDbContext db) =>
{
    var reports = await db.ComplianceReports
        .Where(r => r.SupplierId == supplierId)
        .ToListAsync();
    return Results.Ok(reports);
});

// POST a new compliance check
app.MapPost("/api/compliance", async (ComplianceReport report, ComplianceDbContext db) =>
{
    // Business logic: if score is below 50, it is high risk
    report.IsHighRisk = report.EsgScore < 50;
    report.AssessedDate = DateTime.UtcNow;

    db.ComplianceReports.Add(report);
    await db.SaveChangesAsync();

    // We will add RabbitMQ logic here <<LATER>> to send an alert if IsHighRisk is true!

    return Results.Created($"/api/compliance/{report.SupplierId}", report);
});

// Service works in 5002 port
app.Run("http://localhost:5002");
using ComplianceService;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

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

    // RabbitMQ Publish (Only if High Risk)
    if (report.IsHighRisk)
    {
        // Connect to our Docker RabbitMQ
        var factory = new ConnectionFactory { HostName = "localhost" };

        // Using async methods
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // Create queue if it doesn't exist
        string queueName = "HighRiskVendorQueue";
        await channel.QueueDeclareAsync(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        // Prepare the message
        var messageData = new { SupplierId = report.SupplierId, Score = report.EsgScore };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageData));

        // Send the message
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
    }

    return Results.Created($"/api/compliance/{report.SupplierId}", report);
});

// Service works in 5002 port
app.Run("http://localhost:5002");
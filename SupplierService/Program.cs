using Microsoft.EntityFrameworkCore;
using SupplierService;

var builder = WebApplication.CreateBuilder(args);

// Database Context Inject garne
builder.Services.AddDbContext<SupplierDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// GET all suppliers
app.MapGet("/api/suppliers", async (SupplierDbContext db) =>
    await db.Suppliers.ToListAsync());

// POST 
app.MapPost("/api/suppliers", async (Supplier supplier, SupplierDbContext db) =>
{
    db.Suppliers.Add(supplier);
    await db.SaveChangesAsync();
    return Results.Created($"/api/suppliers/{supplier.Id}", supplier);
});

// Service works in 5001 port
app.Run("http://localhost:5001");
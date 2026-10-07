using NotificationService;

var builder = WebApplication.CreateBuilder(args);

// Register our background worker
builder.Services.AddHostedService<RabbitMqListener>();

var app = builder.Build();

// Health Check: We don't even need HTTP endpoints here
app.MapGet("/", () => "Notification Service is running in the background...");

app.Run("http://localhost:5003");
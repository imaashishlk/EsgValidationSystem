using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Trace;
using System.Threading.RateLimiting;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// 1. Rate Limiting Rule (10 second, max 5 request)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10)
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// YARP configuration from appsettings.json 
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));



builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation() // Gateway ma chhireko request track garcha
               .AddHttpClientInstrumentation()   // Gateway le aru service lai gareko call track garcha
                                                 //.AddConsoleExporter();          // Ahile ko lagi terminal mai visual log dekhaucha
               .AddOtlpExporter(options =>
               {
                   // Docker maa chalirako Jaeger ko address
                   options.Endpoint = new Uri("http://localhost:4317");
               });
    });

var app = builder.Build();

// YARP middleware 
app.MapReverseProxy();
app.UseRateLimiter();

app.Run();
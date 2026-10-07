var builder = WebApplication.CreateBuilder(args);

// YARP configuration from appsettings.json 
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// YARP middleware 
app.MapReverseProxy();

app.Run();
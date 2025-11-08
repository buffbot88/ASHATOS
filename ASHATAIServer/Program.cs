using ASHATAIServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on port 8088
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8088);
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add CORS for ASHAT Goddess client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register LanguageModelService as singleton
builder.Services.AddSingleton<LanguageModelService>();

var app = builder.Build();

// Initialize Language Model Service
var modelService = app.Services.GetRequiredService<LanguageModelService>();
await modelService.InitializeAsync();

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║         ASHATAIServer - AI Processing Server            ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"Server started on port: 8088");
Console.WriteLine($"Server URL: http://localhost:8088");
Console.WriteLine();
Console.WriteLine("Available Endpoints:");
Console.WriteLine("  POST /api/ai/process     - Process AI prompts");
Console.WriteLine("  GET  /api/ai/status      - Get model status");
Console.WriteLine("  POST /api/ai/models/scan - Scan for models");
Console.WriteLine("  GET  /api/ai/health      - Health check");
Console.WriteLine();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();

app.Run();

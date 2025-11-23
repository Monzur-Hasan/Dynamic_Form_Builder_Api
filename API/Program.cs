using BLL;
using BLL.Service.Implementation;
using BLL.Service.Interface;
using Microsoft.OpenApi;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Configuration
// =====================
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Initialize ConfigurationHelper if you use it
ConfigurationHelper.Initialize(builder.Configuration);

// =====================
// Add Services
// =====================
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// =====================
// Register DI for BLL
// =====================
BLLInjector.BLLConfigureServices(builder.Services, builder.Configuration);

// =====================
// Swagger Configuration
// =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dynamic Form Builder API",
        Version = "v1",
        Description = "API for Forms & Options management"
    });
});

var app = builder.Build();

// =====================
// Middleware Pipeline
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dynamic Form Builder API V1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();

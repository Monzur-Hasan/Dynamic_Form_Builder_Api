using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Shared.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Register Configuration
ConfigurationHelper.Initialize(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

var client_Url = builder.Configuration.GetSection("Clients").GetSection("AngularClient").GetSection("Url").Value;

// Configure JWT Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
//        ClockSkew = TimeSpan.Zero
//    };
//});


// Ensure CORS uses runtime configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AngularClient",
        builder =>
        {
            builder.
            WithOrigins(client_Url).
            AllowAnyHeader().
            AllowAnyMethod().
            AllowCredentials();
        });

});

// Register Dependency Injection
BLLInjector.BLLConfigureServices(builder.Services, builder.Configuration);

// Add authentication to Swagger UI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Dynamic Form Builder API", Version = "API V1" });

    options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "JWT"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();
// Enable Swagger
app.UseSwagger();
app.MapOpenApi();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dynamic Form Builder API V1");
});

app.UseHttpsRedirection();

// Enable Static Files & Routing
app.UseStaticFiles();
app.UseRouting();

// Enable CORS
app.UseCors("AngularClient");

// Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

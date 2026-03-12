using KidBank.API.Hubs;
using KidBank.API.Middleware;
using KidBank.Application;
using KidBank.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KidBank API",
        Version = "v1",
        Description = "Family Banking API for Kids"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

await SeedAppSettingsAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "KidBank API v1");
    });
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowSpecificOrigins");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

static async Task SeedAppSettingsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var settingsDb = scope.ServiceProvider.GetRequiredService<KidBank.Infrastructure.Persistence.SettingsDbContext>();

    if (await settingsDb.AppSettings.AnyAsync())
        return;

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var hostname = Environment.MachineName;

    var globals = new (string Key, string Value, string Description)[]
    {
        ("Jwt:Issuer", config["Jwt:Issuer"] ?? "KidBank", "JWT issuer"),
        ("Jwt:Audience", config["Jwt:Audience"] ?? "KidBankApp", "JWT audience"),
        ("Jwt:ExpirationMinutes", config["Jwt:ExpirationMinutes"] ?? "60", "JWT token lifetime in minutes"),
        ("App:DefaultCurrency", "BYN", "Default currency for new accounts"),
        ("App:MaxDailyTopUp", "100000", "Maximum daily top-up amount"),
        ("App:RefreshTokenLifetimeDays", "30", "Refresh token lifetime in days"),
        ("Redis:ConnectionString", config["Redis:ConnectionString"] ?? "localhost:6379", "Redis connection string"),
    };

    foreach (var (key, value, description) in globals)
    {
        settingsDb.AppSettings.Add(
            KidBank.Domain.Services.AppSettingService.Create(key, value, KidBank.Domain.Entities.AppSetting.GlobalHostname, description));
    }

    var hostSettings = new (string Key, string? Value, string Description)[]
    {
        ("ConnectionStrings:DefaultConnection", config.GetConnectionString("DefaultConnection"), "Main database connection string"),
        ("ConnectionStrings:SettingsConnection", config.GetConnectionString("SettingsConnection"), "Settings database connection string"),
    };

    foreach (var (key, value, description) in hostSettings)
    {
        if (!string.IsNullOrEmpty(value))
        {
            settingsDb.AppSettings.Add(
                KidBank.Domain.Services.AppSettingService.Create(key, value, hostname, description));
        }
    }

    await settingsDb.SaveChangesAsync();
}

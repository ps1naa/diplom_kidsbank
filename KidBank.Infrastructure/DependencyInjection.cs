using System.Text;
using KidBank.Application.Common.Interfaces;
using KidBank.Infrastructure.Identity;
using KidBank.Infrastructure.Persistence;
using KidBank.Infrastructure.Security;
using KidBank.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace KidBank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var encryptionKeyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption key is not configured");
        var encryptionKey = Convert.FromBase64String(encryptionKeyBase64);
        var encryptor = new AesGcmDataEncryptor(encryptionKey);
        services.AddSingleton<IDataEncryptor>(encryptor);

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddDbContext<SettingsDbContext>((sp, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("SettingsConnection"),
                b => b.MigrationsAssembly(typeof(SettingsDbContext).Assembly.FullName)));

        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddScoped<CurrentUserService>();
        services.AddScoped<JwtService>();
        services.AddScoped<PasswordHasher>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IAuditLogger, DbAuditLogger>();
        services.AddScoped<DbAppSettingsService>();
        services.AddScoped<RedisSettingsNotifier>();
        services.AddHostedService<RedisSettingsListener>();
        services.AddMemoryCache();

        var jwtSecret = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key is not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = !string.IsNullOrEmpty(configuration["Jwt:Issuer"]),
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = !string.IsNullOrEmpty(configuration["Jwt:Audience"]),
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("ParentOnly", policy => policy.RequireRole("Parent"))
            .AddPolicy("KidOnly", policy => policy.RequireRole("Kid"));

        services.AddHttpContextAccessor();

        return services;
    }
}

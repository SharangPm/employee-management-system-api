using System.Text;
using EmployeeManagement.API.Configuration;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Repositories;
using EmployeeManagement.API.Services;
using MongoDB.Driver;

namespace EmployeeManagement.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoUri = configuration["MONGODB_URI"];
        var databaseName = configuration["MONGODB_DATABASE_NAME"];
        var jwtSecret = configuration["JWT_SECRET"];
        var issuer = configuration["JWT_ISSUER"];
        var audience = configuration["JWT_AUDIENCE"];
        var expiryText = configuration["JWT_EXPIRY_MINUTES"];

        if (string.IsNullOrWhiteSpace(mongoUri))
        {
            throw new InvalidOperationException("MONGODB_URI is required.");
        }
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MONGODB_DATABASE_NAME is required.");
        }
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Contains("CHANGE_ME", StringComparison.Ordinal) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
        {
            throw new InvalidOperationException("JWT_SECRET must contain at least 32 bytes of high-entropy secret material.");
        }
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("JWT_ISSUER and JWT_AUDIENCE are required.");
        }
        if (!int.TryParse(expiryText, out var expiryMinutes) || expiryMinutes is < 5 or > 1440)
        {
            throw new InvalidOperationException("JWT_EXPIRY_MINUTES must be between 5 and 1440.");
        }

        services.AddSingleton(new JwtSettings(jwtSecret, issuer, audience, expiryMinutes));
        var mongoSettings = MongoClientSettings.FromConnectionString(mongoUri);
        mongoSettings.ConnectTimeout = TimeSpan.FromSeconds(5);
        mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(8);
        mongoSettings.WaitQueueTimeout = TimeSpan.FromSeconds(5);

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings));
        services.AddSingleton(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}



using EmployeeManagement.API.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace EmployeeManagement.Tests.Services;

public sealed class MongoClientConfigurationTests
{
    [Fact]
    public void MongoClientUsesConnectionTimeoutsShorterThanFrontendRequestTimeout()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MONGODB_URI"] = "mongodb://localhost:27017",
                ["MONGODB_DATABASE_NAME"] = "EmployeeManagementTests",
                ["JWT_SECRET"] = "test-only-signing-key-with-at-least-32-bytes",
                ["JWT_ISSUER"] = "EmployeeManagementTests",
                ["JWT_AUDIENCE"] = "EmployeeManagementTestClients",
                ["JWT_EXPIRY_MINUTES"] = "60"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddApplicationServices(configuration);

        using var provider = services.BuildServiceProvider();
        var mongoClient = provider.GetRequiredService<IMongoClient>();

        Assert.Equal(TimeSpan.FromSeconds(5), mongoClient.Settings.ConnectTimeout);
        Assert.Equal(TimeSpan.FromSeconds(8), mongoClient.Settings.ServerSelectionTimeout);
        Assert.Equal(TimeSpan.FromSeconds(5), mongoClient.Settings.WaitQueueTimeout);
    }
}

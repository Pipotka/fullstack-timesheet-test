using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Timesheet.Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    private static IConfiguration BuildConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
            ["MongoDb:DatabaseName"] = "TimesheetTest",
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public void AddInfrastructure_RegistersMongoClientAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IMongoClient));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInfrastructure_RegistersMongoDatabaseAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IMongoDatabase));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInfrastructure_ResolvesMongoClient_WithoutNetworkCall()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());
        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IMongoClient>();

        client.Should().NotBeNull();
        client.Settings.Server.Host.Should().Be("localhost");
        client.Settings.Server.Port.Should().Be(27017);
    }

    [Fact]
    public void AddInfrastructure_ResolvesMongoDatabase_WithoutNetworkCall()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());
        var provider = services.BuildServiceProvider();

        var database = provider.GetRequiredService<IMongoDatabase>();

        database.Should().NotBeNull();
        database.DatabaseNamespace.DatabaseName.Should().Be("TimesheetTest");
    }

    [Fact]
    public void AddInfrastructure_ThrowsWhenMongoDbSettingsMissing()
    {
        var emptyConfig = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        var act = () => services.AddInfrastructure(emptyConfig);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MongoDb*");
    }
}

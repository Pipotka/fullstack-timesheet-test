using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Infrastructure.MongoDb.Indexes;

namespace Timesheet.Infrastructure.Tests;

public sealed class RepositoryRegistrationTests
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
    public void AddInfrastructure_RegistersTimeEntryRepository()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ITimeEntryRepository));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().BeAssignableTo<ITimeEntryRepository>();
    }

    [Fact]
    public void AddInfrastructure_RegistersEmployeeRepository()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IEmployeeRepository));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().BeAssignableTo<IEmployeeRepository>();
    }

    [Fact]
    public void AddInfrastructure_RegistersProjectRepository()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IProjectRepository));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().BeAssignableTo<IProjectRepository>();
    }

    [Fact]
    public void AddInfrastructure_RegistersPeriodClosureRepository()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IPeriodClosureRepository));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().BeAssignableTo<IPeriodClosureRepository>();
    }

    [Fact]
    public void AddInfrastructure_RegistersIndexCreator()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(BuildConfiguration());

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IndexCreator));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }
}

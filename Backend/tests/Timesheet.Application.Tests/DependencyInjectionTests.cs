using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Timesheet.Application.Common.Behaviors;
using Timesheet.Application.TimeEntries.List;

namespace Timesheet.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersMediatR()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetService<IMediator>();

        mediator.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_RegistersValidationBehaviorAsPipelineBehavior()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>) &&
            d.ImplementationType == typeof(ValidationBehavior<,>));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddApplication_RegistersValidatorsFromApplicationAssembly()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var validators = scope.ServiceProvider.GetServices<IValidator<ListTimeEntriesQuery>>();

        validators.Should().NotBeNull();
        validators.Should().ContainSingle(v => v is ListTimeEntriesQueryValidator);
    }
}

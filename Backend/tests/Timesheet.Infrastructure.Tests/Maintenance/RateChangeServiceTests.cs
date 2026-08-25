using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;
using Timesheet.Domain.Employees;
using Timesheet.Infrastructure.Maintenance;

namespace Timesheet.Infrastructure.Tests.Maintenance;

public class RateChangeServiceTests
{
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly ITimeEntryRepository _timeEntryRepo = Substitute.For<ITimeEntryRepository>();
    private readonly RateChangeService _service;

    public RateChangeServiceTests()
    {
        _service = new RateChangeService(_employeeRepo, _timeEntryRepo);
    }

    [Fact]
    public async Task ChangeRateAndRecalculate_CallsChangeRateAsync()
    {
        SetupEmployeeWithSingleRate();

        await _service.ChangeRateAndRecalculateAsync("emp-1", new DateOnly(2026, 3, 1), 600, CancellationToken.None);

        await _employeeRepo.Received(1).ChangeRateAsync(
            Arg.Is<EmployeeId>(id => id.Value == "emp-1"),
            Arg.Is<DateOnly>(d => d == new DateOnly(2026, 3, 1)),
            Arg.Is<decimal>(r => r == 600),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeRateAndRecalculate_ReturnsRevision()
    {
        SetupEmployeeWithSingleRate();
        _employeeRepo.ChangeRateAsync(Arg.Any<EmployeeId>(), Arg.Any<DateOnly>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(5L));

        var revision = await _service.ChangeRateAndRecalculateAsync("emp-1", new DateOnly(2026, 3, 1), 600, CancellationToken.None);

        revision.Should().Be(5);
    }

    [Fact]
    public async Task ChangeRateAndRecalculate_CallsUpdateCostsForEachRateInterval()
    {
        var employee = new Employee
        {
            Id = new EmployeeId("emp-1"),
            FullName = "Test",
            RateHistory = new List<RateHistoryEntry>
            {
                new() { From = new DateOnly(2026, 1, 1), Rate = 500 },
                new() { From = new DateOnly(2026, 3, 1), Rate = 600 }
            }.AsReadOnly(),
            RateRevision = 2
        };

        _employeeRepo.ChangeRateAsync(Arg.Any<EmployeeId>(), Arg.Any<DateOnly>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(2L));
        _employeeRepo.GetByIdAsync(Arg.Is<EmployeeId>(id => id.Value == "emp-1"), Arg.Any<CancellationToken>())
            .Returns(employee);

        await _service.ChangeRateAndRecalculateAsync("emp-1", new DateOnly(2026, 3, 1), 600, CancellationToken.None);

        await _timeEntryRepo.Received(2).UpdateCostsByIntervalAsync(
            Arg.Any<EmployeeId>(),
            Arg.Any<DateRange>(),
            Arg.Any<decimal>(),
            Arg.Is<long>(r => r == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeRateAndRecalculate_CallsUpdateCostsAfterChangeRate()
    {
        var callOrder = new List<string>();

        _employeeRepo.ChangeRateAsync(Arg.Any<EmployeeId>(), Arg.Any<DateOnly>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(2L));

        var employee = new Employee
        {
            Id = new EmployeeId("emp-1"),
            FullName = "Test",
            RateHistory = new List<RateHistoryEntry>
            {
                new() { From = new DateOnly(2026, 1, 1), Rate = 500 }
            }.AsReadOnly(),
            RateRevision = 2
        };

        _employeeRepo.GetByIdAsync(Arg.Any<EmployeeId>(), Arg.Any<CancellationToken>())
            .Returns(employee);

        _timeEntryRepo.When(x => x.UpdateCostsByIntervalAsync(
            Arg.Any<EmployeeId>(), Arg.Any<DateRange>(), Arg.Any<decimal>(), Arg.Any<long>(), Arg.Any<CancellationToken>()))
            .Do(_ => callOrder.Add("UpdateCosts"));

        callOrder.Add("ChangeRate");

        await _service.ChangeRateAndRecalculateAsync("emp-1", new DateOnly(2026, 3, 1), 600, CancellationToken.None);

        callOrder.Should().ContainInOrder("ChangeRate", "UpdateCosts");
    }

    [Fact]
    public async Task ChangeRateAndRecalculate_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _employeeRepo.ChangeRateAsync(Arg.Any<EmployeeId>(), Arg.Any<DateOnly>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<long>(new OperationCanceledException(cts.Token)));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.ChangeRateAndRecalculateAsync("emp-1", new DateOnly(2026, 3, 1), 600, cts.Token));
    }

    private void SetupEmployeeWithSingleRate()
    {
        var employee = new Employee
        {
            Id = new EmployeeId("emp-1"),
            FullName = "Test",
            RateHistory = new List<RateHistoryEntry>
            {
                new() { From = new DateOnly(2026, 1, 1), Rate = 500 }
            }.AsReadOnly(),
            RateRevision = 1
        };

        _employeeRepo.ChangeRateAsync(Arg.Any<EmployeeId>(), Arg.Any<DateOnly>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1L));
        _employeeRepo.GetByIdAsync(Arg.Any<EmployeeId>(), Arg.Any<CancellationToken>())
            .Returns(employee);
    }
}

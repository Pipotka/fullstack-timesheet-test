using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Employees.RecalculateCosts;
using Timesheet.Domain;
using Timesheet.Domain.Common;
using Timesheet.Domain.Employees;

namespace Timesheet.Application.Tests.Employees;

public sealed class RecalculateCostsCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();
    private readonly ITimeEntryRepository _timeEntryRepository = Substitute.For<ITimeEntryRepository>();

    private readonly RecalculateCostsCommandHandler _handler;

    public RecalculateCostsCommandHandlerTests()
    {
        _handler = new RecalculateCostsCommandHandler(
            _employeeRepository,
            _timeEntryRepository);
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_ThrowsBusinessException()
    {
        var command = new RecalculateCostsCommand(
            EmployeeId: "emp-001",
            JobRevision: 5);

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.EmployeeNotFound);
    }

    [Fact]
    public async Task Handle_SingleRateEntry_CallsUpdateOnce()
    {
        var command = new RecalculateCostsCommand(
            EmployeeId: "emp-001",
            JobRevision: 5);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 5
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        await _handler.Handle(command, CancellationToken.None);

        await _timeEntryRepository.Received(1).UpdateCostsByIntervalAsync(
            new EmployeeId("emp-001"),
            Arg.Any<DateRange>(),
            1500m,
            5,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleRateEntries_CallsUpdateForEachInterval()
    {
        var command = new RecalculateCostsCommand(
            EmployeeId: "emp-001",
            JobRevision: 5);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory =
            [
                new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1000 },
                new RateHistoryEntry { From = new DateOnly(2026, 4, 1), Rate = 1500 },
                new RateHistoryEntry { From = new DateOnly(2026, 7, 1), Rate = 2000 }
            ],
            RateRevision = 5
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        await _handler.Handle(command, CancellationToken.None);

        await _timeEntryRepository.Received(3).UpdateCostsByIntervalAsync(
            new EmployeeId("emp-001"),
            Arg.Any<DateRange>(),
            Arg.Any<decimal>(),
            5,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesJobRevision_ToRepo()
    {
        var command = new RecalculateCostsCommand(
            EmployeeId: "emp-001",
            JobRevision: 7);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 7
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        await _handler.Handle(command, CancellationToken.None);

        await _timeEntryRepository.Received(1).UpdateCostsByIntervalAsync(
            Arg.Any<EmployeeId>(),
            Arg.Any<DateRange>(),
            Arg.Any<decimal>(),
            7,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesCorrectIntervals_ToRepo()
    {
        var command = new RecalculateCostsCommand(
            EmployeeId: "emp-001",
            JobRevision: 5);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory =
            [
                new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1000 },
                new RateHistoryEntry { From = new DateOnly(2026, 4, 1), Rate = 1500 }
            ],
            RateRevision = 5
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        await _handler.Handle(command, CancellationToken.None);

        // First interval: [2026-01-01, 2026-04-01) with rate 1000
        await _timeEntryRepository.Received(1).UpdateCostsByIntervalAsync(
            new EmployeeId("emp-001"),
            Arg.Is<DateRange>(r => r.From == new DateOnly(2026, 1, 1) && r.To == new DateOnly(2026, 4, 1)),
            1000m,
            5,
            Arg.Any<CancellationToken>());

        // Second interval: [2026-04-01, DateOnly.MaxValue] with rate 1500
        await _timeEntryRepository.Received(1).UpdateCostsByIntervalAsync(
            new EmployeeId("emp-001"),
            Arg.Is<DateRange>(r => r.From == new DateOnly(2026, 4, 1) && r.To == DateOnly.MaxValue),
            1500m,
            5,
            Arg.Any<CancellationToken>());
    }
}

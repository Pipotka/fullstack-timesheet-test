using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Common.Models;
using Timesheet.Application.TimeEntries.List;
using Timesheet.Domain;
using Timesheet.Domain.Common;
using Timesheet.Domain.Employees;
using Timesheet.Domain.Projects;
using Timesheet.Domain.TimeEntries;

namespace Timesheet.Application.Tests.TimeEntries;

public sealed class ListTimeEntriesQueryHandlerTests
{
    private readonly ITimeEntryRepository _timeEntryRepository = Substitute.For<ITimeEntryRepository>();
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();

    private readonly ListTimeEntriesQueryHandler _handler;

    public ListTimeEntriesQueryHandlerTests()
    {
        _handler = new ListTimeEntriesQueryHandler(
            _timeEntryRepository,
            _employeeRepository,
            _projectRepository);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllEntries()
    {
        var query = new ListTimeEntriesQuery();

        var entries = new List<TimeEntry>
        {
            new()
            {
                Id = new TimeEntryId("entry-001"),
                EmployeeId = new EmployeeId("emp-001"),
                ProjectId = new ProjectId("proj-001"),
                Date = new DateOnly(2026, 8, 25),
                Hours = 8.0m,
                Comment = "Test 1",
                AppliedRate = 1500m,
                Cost = 12000m,
                RateRevision = 1,
                Version = 1
            },
            new()
            {
                Id = new TimeEntryId("entry-002"),
                EmployeeId = new EmployeeId("emp-001"),
                ProjectId = new ProjectId("proj-001"),
                Date = new DateOnly(2026, 8, 26),
                Hours = 4.0m,
                Comment = "Test 2",
                AppliedRate = 1500m,
                Cost = 6000m,
                RateRevision = 1,
                Version = 1
            }
        };

        _timeEntryRepository.ListAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((entries, 2));

        _timeEntryRepository.SumByFilterAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((12.0m, 18000.0m));

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 1
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Test Project",
            Budget = 1000000
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithEmployeeFilter_PassesFilterToRepo()
    {
        var query = new ListTimeEntriesQuery(EmployeeId: "emp-001");

        _timeEntryRepository.ListAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((new List<TimeEntry>(), 0));

        _timeEntryRepository.SumByFilterAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((0m, 0m));

        await _handler.Handle(query, CancellationToken.None);

        await _timeEntryRepository.Received(1).ListAsync(
            Arg.Is<TimeEntryFilter>(f => f.EmployeeId == new EmployeeId("emp-001")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CalculatesTotalHoursAndCost()
    {
        var query = new ListTimeEntriesQuery();

        var entries = new List<TimeEntry>
        {
            new()
            {
                Id = new TimeEntryId("entry-001"),
                EmployeeId = new EmployeeId("emp-001"),
                ProjectId = new ProjectId("proj-001"),
                Date = new DateOnly(2026, 8, 25),
                Hours = 8.0m,
                Comment = "Test 1",
                AppliedRate = 1500m,
                Cost = 12000m,
                RateRevision = 1,
                Version = 1
            },
            new()
            {
                Id = new TimeEntryId("entry-002"),
                EmployeeId = new EmployeeId("emp-001"),
                ProjectId = new ProjectId("proj-001"),
                Date = new DateOnly(2026, 8, 26),
                Hours = 4.0m,
                Comment = "Test 2",
                AppliedRate = 1500m,
                Cost = 6000m,
                RateRevision = 1,
                Version = 1
            }
        };

        _timeEntryRepository.ListAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((entries, 2));

        _timeEntryRepository.SumByFilterAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((12.0m, 18000.0m));

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 1
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Test Project",
            Budget = 1000000
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalHours.Should().Be(12.0m);
        result.TotalCost.Should().Be(18000.0m);
    }

    [Fact]
    public async Task Handle_MarksOvertime_WhenDailyHoursExceed12()
    {
        var query = new ListTimeEntriesQuery();

        var entries = new List<TimeEntry>
        {
            new()
            {
                Id = new TimeEntryId("entry-001"),
                EmployeeId = new EmployeeId("emp-001"),
                ProjectId = new ProjectId("proj-001"),
                Date = new DateOnly(2026, 8, 25),
                Hours = 8.0m,
                Comment = "Test 1",
                AppliedRate = 1500m,
                Cost = 12000m,
                RateRevision = 1,
                Version = 1
            },
            new()
            {
                Id = new TimeEntryId("entry-002"),
                EmployeeId = new EmployeeId("emp-001"),
                ProjectId = new ProjectId("proj-001"),
                Date = new DateOnly(2026, 8, 25),
                Hours = 6.0m,
                Comment = "Test 2",
                AppliedRate = 1500m,
                Cost = 9000m,
                RateRevision = 1,
                Version = 1
            }
        };

        _timeEntryRepository.ListAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((entries, 2));

        _timeEntryRepository.SumByFilterAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((14.0m, 21000.0m));

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            null,
            Arg.Any<CancellationToken>())
            .Returns(14.0m);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 1
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Test Project",
            Budget = 1000000
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().AllSatisfy(item => item.IsOvertime.Should().BeTrue());
    }

    [Fact]
    public async Task Handle_DoesNotMarkOvertime_WhenDailyHoursLessOrEqual12()
    {
        var query = new ListTimeEntriesQuery();

        var entries = new List<TimeEntry>
        {
            new()
            {
                Id = new TimeEntryId("entry-001"),
                EmployeeId = new EmployeeId("emp-001"),
                ProjectId = new ProjectId("proj-001"),
                Date = new DateOnly(2026, 8, 25),
                Hours = 8.0m,
                Comment = "Test 1",
                AppliedRate = 1500m,
                Cost = 12000m,
                RateRevision = 1,
                Version = 1
            },
            new()
            {
                Id = new TimeEntryId("entry-002"),
                EmployeeId = new EmployeeId("emp-001"),
                ProjectId = new ProjectId("proj-001"),
                Date = new DateOnly(2026, 8, 25),
                Hours = 4.0m,
                Comment = "Test 2",
                AppliedRate = 1500m,
                Cost = 6000m,
                RateRevision = 1,
                Version = 1
            }
        };

        _timeEntryRepository.ListAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((entries, 2));

        _timeEntryRepository.SumByFilterAsync(Arg.Any<TimeEntryFilter>(), Arg.Any<CancellationToken>())
            .Returns((12.0m, 18000.0m));

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            null,
            Arg.Any<CancellationToken>())
            .Returns(12.0m);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 1
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Test Project",
            Budget = 1000000
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().AllSatisfy(item => item.IsOvertime.Should().BeFalse());
    }
}

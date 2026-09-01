using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.TimeEntries.Create;
using Timesheet.Domain;
using Timesheet.Domain.Employees;
using Timesheet.Domain.PeriodClosures;
using Timesheet.Domain.Projects;
using Timesheet.Domain.TimeEntries;

namespace Timesheet.Application.Tests.TimeEntries;

public sealed class CreateTimeEntryCommandHandlerTests
{
    private readonly ITimeEntryRepository _timeEntryRepository = Substitute.For<ITimeEntryRepository>();
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly IPeriodClosureRepository _periodClosureRepository = Substitute.For<IPeriodClosureRepository>();

    private readonly CreateTimeEntryCommandHandler _handler;

    public CreateTimeEntryCommandHandlerTests()
    {
        _handler = new CreateTimeEntryCommandHandler(
            _timeEntryRepository,
            _employeeRepository,
            _projectRepository,
            _periodClosureRepository);
    }

    [Fact]
    public async Task Handle_PeriodClosed_ThrowsBusinessException()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns(new PeriodClosure { Year = 2026, Month = 8, IsClosed = true });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.PeriodClosed);
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_ThrowsBusinessException()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.EmployeeNotFound);
    }

    [Fact]
    public async Task Handle_ProjectNotFound_ThrowsBusinessException()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 1
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.ProjectNotFound);
    }

    [Fact]
    public async Task Handle_DateOutsideProjectRange_ThrowsBusinessException()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

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
            Budget = 1000000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 7, 31)
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.DateOutsideProjectRange);
    }

    [Fact]
    public async Task Handle_DailyLimitExceeded_ThrowsBusinessException()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 5.0m,
            Comment: "Test");

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

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
            Budget = 1000000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            null,
            Arg.Any<CancellationToken>())
            .Returns(20.0m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.DailyLimitExceeded);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesEntry()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 5
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Test Project",
            Budget = 1000000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            null,
            Arg.Any<CancellationToken>())
            .Returns(0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.EmployeeId.Should().Be("emp-001");
        result.ProjectId.Should().Be("proj-001");
        result.Date.Should().Be(new DateOnly(2026, 8, 25));
        result.Hours.Should().Be(8.0m);
        result.Comment.Should().Be("Test");
        result.Version.Should().Be(1);

        await _timeEntryRepository.Received(1).CreateAsync(
            Arg.Is<TimeEntry>(e =>
                e.EmployeeId == new EmployeeId("emp-001") &&
                e.ProjectId == new ProjectId("proj-001") &&
                e.Date == new DateOnly(2026, 8, 25) &&
                e.Hours == 8.0m &&
                e.Comment == "Test" &&
                e.Version == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CalculatesCostCorrectly()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

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
            Budget = 1000000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            null,
            Arg.Any<CancellationToken>())
            .Returns(0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AppliedRate.Should().Be(1500m);
        result.Cost.Should().Be(12000.00m);
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesEmployeeRateRevision()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

        var employee = new Employee
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Test Employee",
            RateHistory = [new RateHistoryEntry { From = new DateOnly(2026, 1, 1), Rate = 1500 }],
            RateRevision = 5
        };

        _employeeRepository.GetByIdAsync(new EmployeeId("emp-001"), Arg.Any<CancellationToken>())
            .Returns(employee);

        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Test Project",
            Budget = 1000000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };

        _projectRepository.GetByIdAsync(new ProjectId("proj-001"), Arg.Any<CancellationToken>())
            .Returns(project);

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            null,
            Arg.Any<CancellationToken>())
            .Returns(0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.RateRevision.Should().Be(5);
    }
}

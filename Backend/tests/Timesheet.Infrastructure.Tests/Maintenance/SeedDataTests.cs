using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;
using Timesheet.Domain.Employees;
using Timesheet.Domain.Projects;
using Timesheet.Domain.TimeEntries;
using Timesheet.Infrastructure.Maintenance;

namespace Timesheet.Infrastructure.Tests.Maintenance;

public class SeedDataTests
{
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly IProjectRepository _projectRepo = Substitute.For<IProjectRepository>();
    private readonly ITimeEntryRepository _timeEntryRepo = Substitute.For<ITimeEntryRepository>();
    private readonly SeedData _seedData;

    public SeedDataTests()
    {
        _seedData = new SeedData(_employeeRepo, _projectRepo, _timeEntryRepo);
    }

    [Fact]
    public async Task SeedAsync_CreatesEmployees()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<EmployeeId>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _projectRepo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        _timeEntryRepo.GetByIdAsync(Arg.Any<TimeEntryId>(), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        await _seedData.SeedAsync(CancellationToken.None);

        await _employeeRepo.Received(2).CreateAsync(Arg.Is<Employee>(e =>
            e.Id.Value == SeedData.IvanovId || e.Id.Value == SeedData.PetrovaId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedAsync_CreatesProjects()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<EmployeeId>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _projectRepo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        _timeEntryRepo.GetByIdAsync(Arg.Any<TimeEntryId>(), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        await _seedData.SeedAsync(CancellationToken.None);

        await _projectRepo.Received(2).CreateAsync(Arg.Is<Project>(p =>
            p.Id.Value == SeedData.Project001Id || p.Id.Value == SeedData.Project002Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_SkipsExistingEntities()
    {
        var existingEmployee = new Employee
        {
            Id = new EmployeeId(SeedData.IvanovId),
            FullName = "Иванов И.И.",
            RateHistory = new List<RateHistoryEntry>
            {
                new() { From = new DateOnly(2026, 1, 1), Rate = 500 }
            }.AsReadOnly(),
            RateRevision = 1
        };

        _employeeRepo.GetByIdAsync(Arg.Is<EmployeeId>(id => id.Value == SeedData.IvanovId), Arg.Any<CancellationToken>())
            .Returns(existingEmployee);
        _employeeRepo.GetByIdAsync(Arg.Is<EmployeeId>(id => id.Value == SeedData.PetrovaId), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _projectRepo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        _timeEntryRepo.GetByIdAsync(Arg.Any<TimeEntryId>(), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        await _seedData.SeedAsync(CancellationToken.None);

        // Ivanov already exists, should not be created again
        await _employeeRepo.DidNotReceive().CreateAsync(
            Arg.Is<Employee>(e => e.Id.Value == SeedData.IvanovId), Arg.Any<CancellationToken>());
        // Petrova should be created
        await _employeeRepo.Received(1).CreateAsync(
            Arg.Is<Employee>(e => e.Id.Value == SeedData.PetrovaId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedAsync_IvanovHasTwoRateHistoryEntries()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<EmployeeId>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _projectRepo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        _timeEntryRepo.GetByIdAsync(Arg.Any<TimeEntryId>(), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        await _seedData.SeedAsync(CancellationToken.None);

        await _employeeRepo.Received().CreateAsync(
            Arg.Is<Employee>(e =>
                e.Id.Value == SeedData.IvanovId &&
                e.RateHistory.Count == 2 &&
                e.RateHistory[0].Rate == 500 &&
                e.RateHistory[0].From == new DateOnly(2026, 1, 1) &&
                e.RateHistory[1].Rate == 600 &&
                e.RateHistory[1].From == new DateOnly(2026, 3, 1)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedAsync_Project001HasBudget20000()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<EmployeeId>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _projectRepo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        _timeEntryRepo.GetByIdAsync(Arg.Any<TimeEntryId>(), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        await _seedData.SeedAsync(CancellationToken.None);

        await _projectRepo.Received().CreateAsync(
            Arg.Is<Project>(p =>
                p.Id.Value == SeedData.Project001Id &&
                p.Code == "P-001" &&
                p.Budget == 20000 &&
                p.StartDate == new DateOnly(2026, 1, 1) &&
                p.EndDate == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedAsync_Project002HasBudget5000_NoEndDate()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<EmployeeId>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _projectRepo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        _timeEntryRepo.GetByIdAsync(Arg.Any<TimeEntryId>(), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        await _seedData.SeedAsync(CancellationToken.None);

        await _projectRepo.Received().CreateAsync(
            Arg.Is<Project>(p =>
                p.Id.Value == SeedData.Project002Id &&
                p.Code == "P-002" &&
                p.Budget == 5000 &&
                p.StartDate == new DateOnly(2026, 3, 1) &&
                p.EndDate == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedAsync_CreatesFourTimeEntries()
    {
        _employeeRepo.GetByIdAsync(Arg.Any<EmployeeId>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _projectRepo.GetByIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        _timeEntryRepo.GetByIdAsync(Arg.Any<TimeEntryId>(), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        await _seedData.SeedAsync(CancellationToken.None);

        await _timeEntryRepo.Received(4).CreateAsync(Arg.Any<TimeEntry>(), Arg.Any<CancellationToken>());
    }
}

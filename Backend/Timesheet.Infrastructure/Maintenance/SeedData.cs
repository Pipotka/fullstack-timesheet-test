using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;
using Timesheet.Domain.Employees;
using Timesheet.Domain.Projects;
using Timesheet.Domain.TimeEntries;

namespace Timesheet.Infrastructure.Maintenance;

public sealed class SeedData(
    IEmployeeRepository employeeRepository,
    IProjectRepository projectRepository,
    ITimeEntryRepository timeEntryRepository)
{
    public const string IvanovId = "seed-employee-ivanov";
    public const string PetrovaId = "seed-employee-petrova";
    public const string Project001Id = "seed-project-001";
    public const string Project002Id = "seed-project-002";

    private const string Entry1Id = "seed-entry-001";
    private const string Entry2Id = "seed-entry-002";
    private const string Entry3Id = "seed-entry-003";
    private const string Entry4Id = "seed-entry-004";

    public async Task SeedAsync(CancellationToken ct)
    {
        await SeedEmployeesAsync(ct);
        await SeedProjectsAsync(ct);
        await SeedTimeEntriesAsync(ct);
    }

    private async Task SeedEmployeesAsync(CancellationToken ct)
    {
        var ivanov = await employeeRepository.GetByIdAsync(new EmployeeId(IvanovId), ct);
        if (ivanov is null)
        {
            var newIvanov = new Employee
            {
                Id = new EmployeeId(IvanovId),
                FullName = "Иванов И.И.",
                RateHistory = new List<RateHistoryEntry>
                {
                    new() { From = new DateOnly(2026, 1, 1), Rate = 500 },
                    new() { From = new DateOnly(2026, 3, 1), Rate = 600 }
                }.AsReadOnly(),
                RateRevision = 1
            };
            await employeeRepository.CreateAsync(newIvanov, ct);
        }

        var petrova = await employeeRepository.GetByIdAsync(new EmployeeId(PetrovaId), ct);
        if (petrova is null)
        {
            var newPetrova = new Employee
            {
                Id = new EmployeeId(PetrovaId),
                FullName = "Петрова А.С.",
                RateHistory = new List<RateHistoryEntry>
                {
                    new() { From = new DateOnly(2026, 2, 1), Rate = 700 }
                }.AsReadOnly(),
                RateRevision = 1
            };
            await employeeRepository.CreateAsync(newPetrova, ct);
        }
    }

    private async Task SeedProjectsAsync(CancellationToken ct)
    {
        var p001 = await projectRepository.GetByIdAsync(new ProjectId(Project001Id), ct);
        if (p001 is null)
        {
            var newP001 = new Project
            {
                Id = new ProjectId(Project001Id),
                Code = "P-001",
                Name = "Реконструкция цеха",
                Budget = 20000,
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 3, 31)
            };
            await projectRepository.CreateAsync(newP001, ct);
        }

        var p002 = await projectRepository.GetByIdAsync(new ProjectId(Project002Id), ct);
        if (p002 is null)
        {
            var newP002 = new Project
            {
                Id = new ProjectId(Project002Id),
                Code = "P-002",
                Name = "Инженерные сети",
                Budget = 5000,
                StartDate = new DateOnly(2026, 3, 1),
                EndDate = null
            };
            await projectRepository.CreateAsync(newP002, ct);
        }
    }

    private async Task SeedTimeEntriesAsync(CancellationToken ct)
    {
        await CreateEntryIfNotExistsAsync(ct, new TimeEntry
        {
            Id = new TimeEntryId(Entry1Id),
            EmployeeId = new EmployeeId(IvanovId),
            ProjectId = new ProjectId(Project001Id),
            Date = new DateOnly(2026, 2, 20),
            Hours = 8,
            Comment = "Работа по реконструкции (февраль)",
            AppliedRate = 500,
            Cost = 4000,
            RateRevision = 1,
            Version = 1
        });

        await CreateEntryIfNotExistsAsync(ct, new TimeEntry
        {
            Id = new TimeEntryId(Entry2Id),
            EmployeeId = new EmployeeId(IvanovId),
            ProjectId = new ProjectId(Project001Id),
            Date = new DateOnly(2026, 3, 5),
            Hours = 8,
            Comment = "Работа по реконструкции (март)",
            AppliedRate = 600,
            Cost = 4800,
            RateRevision = 1,
            Version = 1
        });

        await CreateEntryIfNotExistsAsync(ct, new TimeEntry
        {
            Id = new TimeEntryId(Entry3Id),
            EmployeeId = new EmployeeId(PetrovaId),
            ProjectId = new ProjectId(Project001Id),
            Date = new DateOnly(2026, 3, 5),
            Hours = 4,
            Comment = "Проектные работы (март)",
            AppliedRate = 700,
            Cost = 2800,
            RateRevision = 1,
            Version = 1
        });

        await CreateEntryIfNotExistsAsync(ct, new TimeEntry
        {
            Id = new TimeEntryId(Entry4Id),
            EmployeeId = new EmployeeId(PetrovaId),
            ProjectId = new ProjectId(Project002Id),
            Date = new DateOnly(2026, 3, 6),
            Hours = 10,
            Comment = "Инженерные изыскания",
            AppliedRate = 700,
            Cost = 7000,
            RateRevision = 1,
            Version = 1
        });
    }

    private async Task CreateEntryIfNotExistsAsync(CancellationToken ct, TimeEntry entry)
    {
        var existing = await timeEntryRepository.GetByIdAsync(entry.Id, ct);
        if (existing is null)
        {
            await timeEntryRepository.CreateAsync(entry, ct);
        }
    }
}

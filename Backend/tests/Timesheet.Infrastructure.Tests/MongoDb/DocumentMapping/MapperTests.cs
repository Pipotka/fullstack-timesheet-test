using FluentAssertions;
using Timesheet.Domain;
using Timesheet.Domain.Employees;
using Timesheet.Domain.PeriodClosures;
using Timesheet.Domain.Projects;
using Timesheet.Domain.TimeEntries;
using Timesheet.Infrastructure.MongoDb.DocumentMapping;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.Tests.MongoDb.DocumentMapping;

public sealed class EmployeeMapperTests
{
    [Fact]
    public void ToDomain_PreservesAllValues()
    {
        var document = new EmployeeDocument
        {
            Id = "emp-001",
            FullName = "John Doe",
            RateHistory =
            [
                new RateHistoryEntryDocument { From = new DateOnly(2024, 1, 1), Rate = 100.50m },
                new RateHistoryEntryDocument { From = new DateOnly(2025, 1, 1), Rate = 150.75m }
            ],
            RateRevision = 42
        };

        var domain = EmployeeMapper.ToDomain(document);

        domain.Id.Value.Should().Be("emp-001");
        domain.FullName.Should().Be("John Doe");
        domain.RateRevision.Should().Be(42);
        domain.RateHistory.Should().HaveCount(2);
        domain.RateHistory[0].From.Should().Be(new DateOnly(2024, 1, 1));
        domain.RateHistory[0].Rate.Should().Be(100.50m);
        domain.RateHistory[1].From.Should().Be(new DateOnly(2025, 1, 1));
        domain.RateHistory[1].Rate.Should().Be(150.75m);
    }

    [Fact]
    public void ToDocument_PreservesAllValues()
    {
        var domain = new Employee
        {
            Id = new EmployeeId("emp-002"),
            FullName = "Jane Smith",
            RateHistory =
            [
                new RateHistoryEntry { From = new DateOnly(2023, 6, 15), Rate = 200.00m }
            ],
            RateRevision = 7
        };

        var document = EmployeeMapper.ToDocument(domain);

        document.Id.Should().Be("emp-002");
        document.FullName.Should().Be("Jane Smith");
        document.RateRevision.Should().Be(7);
        document.RateHistory.Should().HaveCount(1);
        document.RateHistory[0].From.Should().Be(new DateOnly(2023, 6, 15));
        document.RateHistory[0].Rate.Should().Be(200.00m);
    }

    [Fact]
    public void RoundTrip_PreservesAllValues()
    {
        var original = new Employee
        {
            Id = new EmployeeId("emp-003"),
            FullName = "Bob Johnson",
            RateHistory =
            [
                new RateHistoryEntry { From = new DateOnly(2020, 1, 1), Rate = 50.25m },
                new RateHistoryEntry { From = new DateOnly(2022, 7, 1), Rate = 75.50m },
                new RateHistoryEntry { From = new DateOnly(2024, 1, 1), Rate = 100.00m }
            ],
            RateRevision = 99
        };

        var document = EmployeeMapper.ToDocument(original);
        var restored = EmployeeMapper.ToDomain(document);

        restored.Id.Should().Be(original.Id);
        restored.FullName.Should().Be(original.FullName);
        restored.RateRevision.Should().Be(original.RateRevision);
        restored.RateHistory.Should().HaveCount(original.RateHistory.Count);
        for (var i = 0; i < original.RateHistory.Count; i++)
        {
            restored.RateHistory[i].From.Should().Be(original.RateHistory[i].From);
            restored.RateHistory[i].Rate.Should().Be(original.RateHistory[i].Rate);
        }
    }
}

public sealed class ProjectMapperTests
{
    [Fact]
    public void ToDomain_PreservesAllValues()
    {
        var document = new ProjectDocument
        {
            Id = "proj-001",
            Code = "PRJ001",
            Name = "Test Project",
            Budget = 50000.75m,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31)
        };

        var domain = ProjectMapper.ToDomain(document);

        domain.Id.Value.Should().Be("proj-001");
        domain.Code.Should().Be("PRJ001");
        domain.Name.Should().Be("Test Project");
        domain.Budget.Should().Be(50000.75m);
        domain.StartDate.Should().Be(new DateOnly(2024, 1, 1));
        domain.EndDate.Should().Be(new DateOnly(2024, 12, 31));
    }

    [Fact]
    public void ToDomain_PreservesNullableEndDate()
    {
        var document = new ProjectDocument
        {
            Id = "proj-002",
            Code = "PRJ002",
            Name = "Ongoing Project",
            Budget = 100000.00m,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = null
        };

        var domain = ProjectMapper.ToDomain(document);

        domain.EndDate.Should().BeNull();
    }

    [Fact]
    public void ToDocument_PreservesAllValues()
    {
        var domain = new Project
        {
            Id = new ProjectId("proj-003"),
            Code = "PRJ003",
            Name = "Another Project",
            Budget = 25000.50m,
            StartDate = new DateOnly(2025, 3, 1),
            EndDate = new DateOnly(2025, 9, 30)
        };

        var document = ProjectMapper.ToDocument(domain);

        document.Id.Should().Be("proj-003");
        document.Code.Should().Be("PRJ003");
        document.Name.Should().Be("Another Project");
        document.Budget.Should().Be(25000.50m);
        document.StartDate.Should().Be(new DateOnly(2025, 3, 1));
        document.EndDate.Should().Be(new DateOnly(2025, 9, 30));
    }

    [Fact]
    public void RoundTrip_PreservesAllValues_WithEndDate()
    {
        var original = new Project
        {
            Id = new ProjectId("proj-004"),
            Code = "PRJ004",
            Name = "Complete Project",
            Budget = 75000.99m,
            StartDate = new DateOnly(2023, 1, 1),
            EndDate = new DateOnly(2023, 12, 31)
        };

        var document = ProjectMapper.ToDocument(original);
        var restored = ProjectMapper.ToDomain(document);

        restored.Id.Should().Be(original.Id);
        restored.Code.Should().Be(original.Code);
        restored.Name.Should().Be(original.Name);
        restored.Budget.Should().Be(original.Budget);
        restored.StartDate.Should().Be(original.StartDate);
        restored.EndDate.Should().Be(original.EndDate);
    }

    [Fact]
    public void RoundTrip_PreservesAllValues_WithoutEndDate()
    {
        var original = new Project
        {
            Id = new ProjectId("proj-005"),
            Code = "PRJ005",
            Name = "Open-ended Project",
            Budget = 30000.00m,
            StartDate = new DateOnly(2024, 6, 1),
            EndDate = null
        };

        var document = ProjectMapper.ToDocument(original);
        var restored = ProjectMapper.ToDomain(document);

        restored.Id.Should().Be(original.Id);
        restored.Code.Should().Be(original.Code);
        restored.Name.Should().Be(original.Name);
        restored.Budget.Should().Be(original.Budget);
        restored.StartDate.Should().Be(original.StartDate);
        restored.EndDate.Should().BeNull();
    }
}

public sealed class TimeEntryMapperTests
{
    [Fact]
    public void ToDomain_PreservesAllValues()
    {
        var document = new TimeEntryDocument
        {
            Id = "te-001",
            EmployeeId = "emp-001",
            ProjectId = "proj-001",
            Date = new DateOnly(2024, 8, 25),
            Hours = 8.5m,
            Comment = "Worked on feature X",
            AppliedRate = 150.00m,
            Cost = 1275.00m,
            RateRevision = 5,
            Version = 3
        };

        var domain = TimeEntryMapper.ToDomain(document);

        domain.Id.Value.Should().Be("te-001");
        domain.EmployeeId.Value.Should().Be("emp-001");
        domain.ProjectId.Value.Should().Be("proj-001");
        domain.Date.Should().Be(new DateOnly(2024, 8, 25));
        domain.Hours.Should().Be(8.5m);
        domain.Comment.Should().Be("Worked on feature X");
        domain.AppliedRate.Should().Be(150.00m);
        domain.Cost.Should().Be(1275.00m);
        domain.RateRevision.Should().Be(5);
        domain.Version.Should().Be(3);
    }

    [Fact]
    public void ToDocument_PreservesAllValues()
    {
        var domain = new TimeEntry
        {
            Id = new TimeEntryId("te-002"),
            EmployeeId = new EmployeeId("emp-002"),
            ProjectId = new ProjectId("proj-002"),
            Date = new DateOnly(2024, 9, 15),
            Hours = 4.0m,
            Comment = "Meeting",
            AppliedRate = 200.00m,
            Cost = 800.00m,
            RateRevision = 10,
            Version = 1
        };

        var document = TimeEntryMapper.ToDocument(domain);

        document.Id.Should().Be("te-002");
        document.EmployeeId.Should().Be("emp-002");
        document.ProjectId.Should().Be("proj-002");
        document.Date.Should().Be(new DateOnly(2024, 9, 15));
        document.Hours.Should().Be(4.0m);
        document.Comment.Should().Be("Meeting");
        document.AppliedRate.Should().Be(200.00m);
        document.Cost.Should().Be(800.00m);
        document.RateRevision.Should().Be(10);
        document.Version.Should().Be(1);
    }

    [Fact]
    public void RoundTrip_PreservesAllValues()
    {
        var original = new TimeEntry
        {
            Id = new TimeEntryId("te-003"),
            EmployeeId = new EmployeeId("emp-003"),
            ProjectId = new ProjectId("proj-003"),
            Date = new DateOnly(2024, 10, 1),
            Hours = 6.5m,
            Comment = "Development work",
            AppliedRate = 175.50m,
            Cost = 1140.75m,
            RateRevision = 15,
            Version = 7
        };

        var document = TimeEntryMapper.ToDocument(original);
        var restored = TimeEntryMapper.ToDomain(document);

        restored.Id.Should().Be(original.Id);
        restored.EmployeeId.Should().Be(original.EmployeeId);
        restored.ProjectId.Should().Be(original.ProjectId);
        restored.Date.Should().Be(original.Date);
        restored.Hours.Should().Be(original.Hours);
        restored.Comment.Should().Be(original.Comment);
        restored.AppliedRate.Should().Be(original.AppliedRate);
        restored.Cost.Should().Be(original.Cost);
        restored.RateRevision.Should().Be(original.RateRevision);
        restored.Version.Should().Be(original.Version);
    }

    [Fact]
    public void RoundTrip_PreservesDecimalPrecision()
    {
        var original = new TimeEntry
        {
            Id = new TimeEntryId("te-004"),
            EmployeeId = new EmployeeId("emp-004"),
            ProjectId = new ProjectId("proj-004"),
            Date = new DateOnly(2024, 11, 1),
            Hours = 0.5m,
            Comment = "Quick task",
            AppliedRate = 999999.99m,
            Cost = 499999.995m,
            RateRevision = 1,
            Version = 1
        };

        var document = TimeEntryMapper.ToDocument(original);
        var restored = TimeEntryMapper.ToDomain(document);

        restored.Hours.Should().Be(0.5m);
        restored.AppliedRate.Should().Be(999999.99m);
        restored.Cost.Should().Be(499999.995m);
    }
}

public sealed class PeriodClosureMapperTests
{
    [Fact]
    public void ToDomain_PreservesAllValues()
    {
        var document = new PeriodClosureDocument
        {
            Year = 2024,
            Month = 8,
            IsClosed = true
        };

        var domain = PeriodClosureMapper.ToDomain(document);

        domain.Year.Should().Be(2024);
        domain.Month.Should().Be(8);
        domain.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void ToDocument_PreservesAllValues()
    {
        var domain = new PeriodClosure
        {
            Year = 2025,
            Month = 12,
            IsClosed = false
        };

        var document = PeriodClosureMapper.ToDocument(domain);

        document.Year.Should().Be(2025);
        document.Month.Should().Be(12);
        document.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void RoundTrip_PreservesAllValues()
    {
        var original = new PeriodClosure
        {
            Year = 2023,
            Month = 1,
            IsClosed = true
        };

        var document = PeriodClosureMapper.ToDocument(original);
        var restored = PeriodClosureMapper.ToDomain(document);

        restored.Year.Should().Be(original.Year);
        restored.Month.Should().Be(original.Month);
        restored.IsClosed.Should().Be(original.IsClosed);
    }
}

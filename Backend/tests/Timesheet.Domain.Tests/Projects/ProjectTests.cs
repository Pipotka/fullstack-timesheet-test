using FluentAssertions;
using Timesheet.Domain.Projects;

namespace Timesheet.Domain.Tests.Projects;

public sealed class ProjectTests
{
    [Fact]
    public void Project_CanBeCreated_WithValidData()
    {
        var act = () => new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Проект 1",
            Budget = 2_000_000m,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Project_CanBeCreated_WithNullDates()
    {
        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Проект 1",
            Budget = 2_000_000m,
            StartDate = null,
            EndDate = null
        };

        project.StartDate.Should().BeNull();
        project.EndDate.Should().BeNull();
    }

    [Fact]
    public void Project_CanBeCreated_WithZeroBudget()
    {
        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Проект 1",
            Budget = 0m
        };

        project.Budget.Should().Be(0m);
    }

    [Fact]
    public void Project_CanBeCreated_WithOnlyStartDate()
    {
        var project = new Project
        {
            Id = new ProjectId("proj-001"),
            Code = "PRJ-001",
            Name = "Проект 1",
            Budget = 100m,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = null
        };

        project.StartDate.Should().NotBeNull();
        project.EndDate.Should().BeNull();
    }
}

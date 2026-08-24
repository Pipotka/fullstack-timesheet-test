using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Common.Models;
using Timesheet.Application.Reports.ProjectReport;
using Timesheet.Domain;

namespace Timesheet.Application.Tests.Reports;

public sealed class ProjectReportQueryHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();

    private readonly ProjectReportQueryHandler _handler;

    public ProjectReportQueryHandlerTests()
    {
        _handler = new ProjectReportQueryHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_CalculatesUtilizationPercent()
    {
        var reports = new List<ProjectReportResult>
        {
            new(
                ProjectId: new ProjectId("proj-001"),
                ProjectCode: "PRJ-001",
                ProjectName: "Project 1",
                Budget: 2000000m,
                TotalHours: 1000m,
                TotalCost: 1800000m)
        };

        _projectRepository.GetReportsByPeriodAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns(reports);

        var result = await _handler.Handle(new ProjectReportQuery(2026, 8), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].UtilizationPercent.Should().Be(90.00m);
    }

    [Fact]
    public async Task Handle_ZeroBudget_UtilizationPercentIsZero()
    {
        var reports = new List<ProjectReportResult>
        {
            new(
                ProjectId: new ProjectId("proj-001"),
                ProjectCode: "PRJ-001",
                ProjectName: "Project 1",
                Budget: 0m,
                TotalHours: 100m,
                TotalCost: 50000m)
        };

        _projectRepository.GetReportsByPeriodAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns(reports);

        var result = await _handler.Handle(new ProjectReportQuery(2026, 8), CancellationToken.None);

        result[0].UtilizationPercent.Should().Be(0m);
        result[0].IsAtRisk.Should().BeFalse();
        result[0].IsOverrun.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UtilizationAbove80_IsAtRiskTrue()
    {
        var reports = new List<ProjectReportResult>
        {
            new(
                ProjectId: new ProjectId("proj-001"),
                ProjectCode: "PRJ-001",
                ProjectName: "Project 1",
                Budget: 2000000m,
                TotalHours: 1000m,
                TotalCost: 1800000m)
        };

        _projectRepository.GetReportsByPeriodAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns(reports);

        var result = await _handler.Handle(new ProjectReportQuery(2026, 8), CancellationToken.None);

        result[0].IsAtRisk.Should().BeTrue();
        result[0].IsOverrun.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UtilizationAbove100_IsOverrunTrue()
    {
        var reports = new List<ProjectReportResult>
        {
            new(
                ProjectId: new ProjectId("proj-001"),
                ProjectCode: "PRJ-001",
                ProjectName: "Project 1",
                Budget: 1000000m,
                TotalHours: 1000m,
                TotalCost: 1100000m)
        };

        _projectRepository.GetReportsByPeriodAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns(reports);

        var result = await _handler.Handle(new ProjectReportQuery(2026, 8), CancellationToken.None);

        result[0].UtilizationPercent.Should().Be(110.00m);
        result[0].IsAtRisk.Should().BeTrue();
        result[0].IsOverrun.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UtilizationBelow80_NoFlags()
    {
        var reports = new List<ProjectReportResult>
        {
            new(
                ProjectId: new ProjectId("proj-001"),
                ProjectCode: "PRJ-001",
                ProjectName: "Project 1",
                Budget: 2000000m,
                TotalHours: 500m,
                TotalCost: 1000000m)
        };

        _projectRepository.GetReportsByPeriodAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns(reports);

        var result = await _handler.Handle(new ProjectReportQuery(2026, 8), CancellationToken.None);

        result[0].UtilizationPercent.Should().Be(50.00m);
        result[0].IsAtRisk.Should().BeFalse();
        result[0].IsOverrun.Should().BeFalse();
    }
}

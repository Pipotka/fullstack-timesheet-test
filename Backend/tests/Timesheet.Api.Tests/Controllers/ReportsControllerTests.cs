using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Timesheet.Api.Controllers;
using Timesheet.Application.Reports.ProjectReport;

namespace Timesheet.Api.Tests.Controllers;

public class ReportsControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ReportsController _controller;

    public ReportsControllerTests()
    {
        _controller = new ReportsController(_mediator);
    }

    [Fact]
    public async Task GetProjectReport_ReturnsOk()
    {
        var items = new List<ProjectReportItem>
        {
            new("proj-1", "P-001", "Reconstruction", 20000, 100, 50000, 250.00m, true, true)
        };

        _mediator.Send(Arg.Any<ProjectReportQuery>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var result = await _controller.GetProjectReport(2026, 1, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetProjectReport_PassesYearMonth()
    {
        ProjectReportQuery? captured = null;
        _mediator.Send(Arg.Any<ProjectReportQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                captured = callInfo.Arg<ProjectReportQuery>();
                return new List<ProjectReportItem>();
            });

        await _controller.GetProjectReport(2026, 3, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Year.Should().Be(2026);
        captured.Month.Should().Be(3);
    }
}

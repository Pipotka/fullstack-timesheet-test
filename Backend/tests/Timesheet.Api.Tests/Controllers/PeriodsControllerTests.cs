using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Timesheet.Api.Controllers;
using Timesheet.Api.Contracts;
using Timesheet.Application.PeriodClosures.Close;
using Timesheet.Application.PeriodClosures.Open;

namespace Timesheet.Api.Tests.Controllers;

public class PeriodsControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly PeriodsController _controller;

    public PeriodsControllerTests()
    {
        _controller = new PeriodsController(_mediator);
    }

    [Fact]
    public async Task Close_ReturnsOk()
    {
        _mediator.Send(Arg.Any<ClosePeriodCommand>(), Arg.Any<CancellationToken>())
            .Returns(new PeriodResult(2026, 1, true));

        var request = new PeriodRequest(2026, 1);
        var result = await _controller.Close(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Close_MapsRequestToCommand()
    {
        ClosePeriodCommand? captured = null;
        _mediator.Send(Arg.Any<ClosePeriodCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                captured = callInfo.Arg<ClosePeriodCommand>();
                return new PeriodResult(2026, 1, true);
            });

        var request = new PeriodRequest(2026, 3);
        await _controller.Close(request, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Year.Should().Be(2026);
        captured.Month.Should().Be(3);
    }

    [Fact]
    public async Task Open_ReturnsOk()
    {
        _mediator.Send(Arg.Any<OpenPeriodCommand>(), Arg.Any<CancellationToken>())
            .Returns(new PeriodResult(2026, 1, false));

        var request = new PeriodRequest(2026, 1);
        var result = await _controller.Open(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Open_MapsRequestToCommand()
    {
        OpenPeriodCommand? captured = null;
        _mediator.Send(Arg.Any<OpenPeriodCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                captured = callInfo.Arg<OpenPeriodCommand>();
                return new PeriodResult(2026, 1, false);
            });

        var request = new PeriodRequest(2026, 2);
        await _controller.Open(request, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Year.Should().Be(2026);
        captured.Month.Should().Be(2);
    }
}

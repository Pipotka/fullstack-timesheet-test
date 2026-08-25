using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Timesheet.Api.Controllers;
using Timesheet.Api.Contracts;
using Timesheet.Application.TimeEntries.Create;
using Timesheet.Application.TimeEntries.Update;
using Timesheet.Application.TimeEntries.Delete;
using Timesheet.Application.TimeEntries.List;

namespace Timesheet.Api.Tests.Controllers;

public class TimeEntriesControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly TimeEntriesController _controller;

    public TimeEntriesControllerTests()
    {
        _controller = new TimeEntriesController(_mediator);
    }

    [Fact]
    public async Task GetList_ReturnsOk_WithPaginatedResult()
    {
        var expectedResult = new ListTimeEntriesResult(
            Items: new List<TimeEntryItem>(),
            TotalCount: 0,
            Page: 1,
            PageSize: 50,
            TotalHours: 0,
            TotalCost: 0);

        _mediator.Send(Arg.Any<ListTimeEntriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var result = await _controller.GetList(2026, 1, null, null, null, null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetList_PassesYearMonth_AsDateRange()
    {
        ListTimeEntriesQuery? capturedQuery = null;
        _mediator.Send(Arg.Any<ListTimeEntriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedQuery = callInfo.Arg<ListTimeEntriesQuery>();
                return new ListTimeEntriesResult([], 0, 1, 50, 0, 0);
            });

        await _controller.GetList(2026, 3, null, null, null, null, CancellationToken.None);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.FromDate.Should().Be(new DateOnly(2026, 3, 1));
        capturedQuery.ToDate.Should().Be(new DateOnly(2026, 3, 31));
    }

    [Fact]
    public async Task Post_ReturnsCreated_WithResult()
    {
        var commandResult = new CreateTimeEntryResult(
            "id-1", "emp-1", "proj-1",
            new DateOnly(2026, 1, 15), 8, "test",
            500, 4000, 1, 1);

        _mediator.Send(Arg.Any<CreateTimeEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        var request = new CreateTimeEntryRequest("emp-1", "proj-1", "2026-01-15", 8, "test");
        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        var created = (CreatedAtActionResult)result;
        created.ActionName.Should().Be(nameof(TimeEntriesController.GetList));
    }

    [Fact]
    public async Task Put_ReturnsOk_WithUpdatedResult()
    {
        var commandResult = new UpdateTimeEntryResult(
            "id-1", "emp-1", "proj-1",
            new DateOnly(2026, 1, 15), 4, "updated",
            500, 2000, 1, 2);

        _mediator.Send(Arg.Any<UpdateTimeEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(commandResult);

        var request = new UpdateTimeEntryRequest(1, 4, "updated");
        var result = await _controller.Update("id-1", request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _mediator.Send(Arg.Any<DeleteTimeEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete("id-1", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Post_MapsRequestToCommand()
    {
        CreateTimeEntryCommand? captured = null;
        _mediator.Send(Arg.Any<CreateTimeEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                captured = callInfo.Arg<CreateTimeEntryCommand>();
                return new CreateTimeEntryResult("id", "emp", "proj", default, 0, "", 0, 0, 0, 0);
            });

        var request = new CreateTimeEntryRequest("emp-1", "proj-1", "2026-02-15", 8.5m, "comment");
        await _controller.Create(request, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.EmployeeId.Should().Be("emp-1");
        captured.ProjectId.Should().Be("proj-1");
        captured.Date.Should().Be(new DateOnly(2026, 2, 15));
        captured.Hours.Should().Be(8.5m);
        captured.Comment.Should().Be("comment");
    }

    [Fact]
    public async Task Put_MapsIdFromRoute()
    {
        UpdateTimeEntryCommand? captured = null;
        _mediator.Send(Arg.Any<UpdateTimeEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                captured = callInfo.Arg<UpdateTimeEntryCommand>();
                return new UpdateTimeEntryResult("id-1", "emp", "proj", default, 0, "", 0, 0, 0, 0);
            });

        var request = new UpdateTimeEntryRequest(5, 4, "test");
        await _controller.Update("route-id", request, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Id.Should().Be("route-id");
        captured.Version.Should().Be(5);
        captured.Hours.Should().Be(4);
    }
}

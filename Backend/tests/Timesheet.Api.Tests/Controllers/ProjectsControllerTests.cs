using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Timesheet.Api.Controllers;
using Timesheet.Application.Projects.List;

namespace Timesheet.Api.Tests.Controllers;

public class ProjectsControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ProjectsController _controller;

    public ProjectsControllerTests()
    {
        _controller = new ProjectsController(_mediator);
    }

    [Fact]
    public async Task GetList_ReturnsOk()
    {
        var items = new List<ProjectListItem>
        {
            new("proj-1", "P-001", "Reconstruction", 20000, new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31))
        };

        _mediator.Send(Arg.Any<ListProjectsQuery>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var result = await _controller.GetList(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetList_DelegatesToMediator()
    {
        _mediator.Send(Arg.Any<ListProjectsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProjectListItem>());

        await _controller.GetList(CancellationToken.None);

        await _mediator.Received(1).Send(Arg.Any<ListProjectsQuery>(), Arg.Any<CancellationToken>());
    }
}

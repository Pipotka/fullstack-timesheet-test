using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Timesheet.Api.Controllers;
using Timesheet.Application.Employees.List;

namespace Timesheet.Api.Tests.Controllers;

public class EmployeesControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _controller = new EmployeesController(_mediator);
    }

    [Fact]
    public async Task GetList_ReturnsOk()
    {
        var items = new List<EmployeeListItem>
        {
            new("emp-1", "Ivanov I.I.", 600m)
        };

        _mediator.Send(Arg.Any<ListEmployeesQuery>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var result = await _controller.GetList(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetList_DelegatesToMediator()
    {
        _mediator.Send(Arg.Any<ListEmployeesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<EmployeeListItem>());

        await _controller.GetList(CancellationToken.None);

        await _mediator.Received(1).Send(Arg.Any<ListEmployeesQuery>(), Arg.Any<CancellationToken>());
    }
}

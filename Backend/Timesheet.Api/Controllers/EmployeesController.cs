using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Application.Employees.List;

namespace Timesheet.Api.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct)
    {
        var query = new ListEmployeesQuery();
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }
}

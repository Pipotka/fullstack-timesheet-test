using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Application.Projects.List;

namespace Timesheet.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct)
    {
        var query = new ListProjectsQuery();
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }
}

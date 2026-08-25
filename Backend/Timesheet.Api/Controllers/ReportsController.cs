using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Application.Reports.ProjectReport;

namespace Timesheet.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("projects")]
    public async Task<IActionResult> GetProjectReport(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        var query = new ProjectReportQuery(year, month);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }
}

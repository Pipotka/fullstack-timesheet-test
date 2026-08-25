using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Api.Contracts;
using Timesheet.Application.PeriodClosures.Close;
using Timesheet.Application.PeriodClosures.Open;

namespace Timesheet.Api.Controllers;

[ApiController]
[Route("api/periods")]
public sealed class PeriodsController(IMediator mediator) : ControllerBase
{
    [HttpPost("close")]
    public async Task<IActionResult> Close(
        [FromBody] PeriodRequest request,
        CancellationToken ct)
    {
        var command = new ClosePeriodCommand(request.Year, request.Month);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("open")]
    public async Task<IActionResult> Open(
        [FromBody] PeriodRequest request,
        CancellationToken ct)
    {
        var command = new OpenPeriodCommand(request.Year, request.Month);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }
}

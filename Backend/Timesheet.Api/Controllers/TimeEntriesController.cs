using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Api.Contracts;
using Timesheet.Application.TimeEntries.Create;
using Timesheet.Application.TimeEntries.Delete;
using Timesheet.Application.TimeEntries.List;
using Timesheet.Application.TimeEntries.Update;

namespace Timesheet.Api.Controllers;

[ApiController]
[Route("api/time-entries")]
public sealed class TimeEntriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string? employeeId,
        [FromQuery] string? projectId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var fromDate = new DateOnly(year, month, 1);
        var toDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        var query = new ListTimeEntriesQuery(
            EmployeeId: employeeId,
            ProjectId: projectId,
            FromDate: fromDate,
            ToDate: toDate,
            Page: page ?? 1,
            PageSize: pageSize ?? 50);

        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTimeEntryRequest request,
        CancellationToken ct)
    {
        var date = DateOnly.ParseExact(request.Date, "yyyy-MM-dd");

        var command = new CreateTimeEntryCommand(
            request.EmployeeId,
            request.ProjectId,
            date,
            request.Hours,
            request.Comment);

        var result = await mediator.Send(command, ct);

        return CreatedAtAction(
            nameof(GetList),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] UpdateTimeEntryRequest request,
        CancellationToken ct)
    {
        var command = new UpdateTimeEntryCommand(
            id,
            request.Version,
            request.Hours,
            request.Comment);

        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken ct)
    {
        var command = new DeleteTimeEntryCommand(id);
        await mediator.Send(command, ct);
        return NoContent();
    }
}

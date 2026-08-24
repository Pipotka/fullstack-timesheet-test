using MediatR;

namespace Timesheet.Application.TimeEntries.Update;

public sealed record UpdateTimeEntryCommand(
    string Id,
    long Version,
    decimal Hours,
    string Comment) : IRequest<UpdateTimeEntryResult>;

public sealed record UpdateTimeEntryResult(
    string Id,
    string EmployeeId,
    string ProjectId,
    DateOnly Date,
    decimal Hours,
    string Comment,
    decimal AppliedRate,
    decimal Cost,
    long RateRevision,
    long Version);

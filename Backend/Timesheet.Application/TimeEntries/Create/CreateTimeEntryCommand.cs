using MediatR;

namespace Timesheet.Application.TimeEntries.Create;

public sealed record CreateTimeEntryCommand(
    string EmployeeId,
    string ProjectId,
    DateOnly Date,
    decimal Hours,
    string Comment) : IRequest<CreateTimeEntryResult>;

public sealed record CreateTimeEntryResult(
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

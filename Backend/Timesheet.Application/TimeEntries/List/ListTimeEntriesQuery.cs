using MediatR;

namespace Timesheet.Application.TimeEntries.List;

public sealed record ListTimeEntriesQuery(
    string? EmployeeId = null,
    string? ProjectId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 50) : IRequest<ListTimeEntriesResult>;

public sealed record ListTimeEntriesResult(
    IReadOnlyList<TimeEntryItem> Items,
    int TotalCount,
    int Page,
    int PageSize,
    decimal TotalHours,
    decimal TotalCost);

public sealed record TimeEntryItem(
    string Id,
    string EmployeeId,
    string EmployeeName,
    string ProjectId,
    string ProjectCode,
    DateOnly Date,
    decimal Hours,
    string Comment,
    decimal AppliedRate,
    decimal Cost,
    bool IsOvertime);

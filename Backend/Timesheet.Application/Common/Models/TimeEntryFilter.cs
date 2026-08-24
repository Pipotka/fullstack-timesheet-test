namespace Timesheet.Application.Common.Models;

public sealed record TimeEntryFilter(
    Timesheet.Domain.EmployeeId? EmployeeId = null,
    Timesheet.Domain.ProjectId? ProjectId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 50);

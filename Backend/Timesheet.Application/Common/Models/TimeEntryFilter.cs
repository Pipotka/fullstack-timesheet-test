namespace Timesheet.Application.Common.Models;

public sealed record TimeEntryFilter(
    Domain.EmployeeId? EmployeeId = null,
    Domain.ProjectId? ProjectId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 50);

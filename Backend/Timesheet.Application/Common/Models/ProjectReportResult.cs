namespace Timesheet.Application.Common.Models;

public sealed record ProjectReportResult(
    Timesheet.Domain.ProjectId ProjectId,
    string ProjectCode,
    string ProjectName,
    decimal Budget,
    decimal TotalHours,
    decimal TotalCost);

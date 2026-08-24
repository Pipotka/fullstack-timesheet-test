using MediatR;

namespace Timesheet.Application.Reports.ProjectReport;

public sealed record ProjectReportQuery(int Year, int Month) : IRequest<IReadOnlyList<ProjectReportItem>>;

public sealed record ProjectReportItem(
    string ProjectId,
    string ProjectCode,
    string ProjectName,
    decimal Budget,
    decimal TotalHours,
    decimal TotalCost,
    decimal UtilizationPercent,
    bool IsAtRisk,
    bool IsOverrun);

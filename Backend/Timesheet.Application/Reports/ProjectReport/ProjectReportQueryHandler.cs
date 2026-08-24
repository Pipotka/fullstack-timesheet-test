using MediatR;
using Timesheet.Application.Common.Interfaces;

namespace Timesheet.Application.Reports.ProjectReport;

public sealed class ProjectReportQueryHandler(
    IProjectRepository projectRepository)
    : IRequestHandler<ProjectReportQuery, IReadOnlyList<ProjectReportItem>>
{
    public async Task<IReadOnlyList<ProjectReportItem>> Handle(
        ProjectReportQuery query,
        CancellationToken cancellationToken)
    {
        var reports = await projectRepository.GetReportsByPeriodAsync(
            query.Year,
            query.Month,
            cancellationToken);

        return reports
            .Select(r =>
            {
                var utilization = r.Budget > 0
                    ? Math.Round(r.TotalCost / r.Budget * 100, 2, MidpointRounding.AwayFromZero)
                    : 0m;

                return new ProjectReportItem(
                    r.ProjectId.Value,
                    r.ProjectCode,
                    r.ProjectName,
                    r.Budget,
                    r.TotalHours,
                    r.TotalCost,
                    utilization,
                    utilization > 80,
                    utilization > 100);
            })
            .ToList();
    }
}

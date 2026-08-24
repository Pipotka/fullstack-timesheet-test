using Timesheet.Domain;
using Timesheet.Domain.Projects;
using Timesheet.Application.Common.Models;

namespace Timesheet.Application.Common.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct);
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<ProjectReportResult>> GetReportsByPeriodAsync(int year, int month, CancellationToken ct);
}

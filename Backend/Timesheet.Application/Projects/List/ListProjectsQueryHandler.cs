using MediatR;
using Timesheet.Application.Common.Interfaces;

namespace Timesheet.Application.Projects.List;

public sealed class ListProjectsQueryHandler(
    IProjectRepository projectRepository)
    : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectListItem>>
{
    public async Task<IReadOnlyList<ProjectListItem>> Handle(
        ListProjectsQuery query,
        CancellationToken cancellationToken)
    {
        var projects = await projectRepository.ListAsync(cancellationToken);

        return projects
            .Select(p => new ProjectListItem(
                p.Id.Value,
                p.Code,
                p.Name,
                p.Budget,
                p.StartDate,
                p.EndDate))
            .ToList();
    }
}

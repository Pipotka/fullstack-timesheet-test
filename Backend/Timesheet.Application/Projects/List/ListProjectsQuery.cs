using MediatR;

namespace Timesheet.Application.Projects.List;

public sealed record ListProjectsQuery : IRequest<IReadOnlyList<ProjectListItem>>;

public sealed record ProjectListItem(
    string Id,
    string Code,
    string Name,
    decimal Budget,
    DateOnly? StartDate,
    DateOnly? EndDate);

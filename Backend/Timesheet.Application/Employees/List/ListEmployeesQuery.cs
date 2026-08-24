using MediatR;

namespace Timesheet.Application.Employees.List;

public sealed record ListEmployeesQuery : IRequest<IReadOnlyList<EmployeeListItem>>;

public sealed record EmployeeListItem(
    string Id,
    string FullName,
    decimal CurrentRate);

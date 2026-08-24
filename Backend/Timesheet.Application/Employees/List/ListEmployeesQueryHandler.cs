using MediatR;
using Timesheet.Application.Common.Interfaces;

namespace Timesheet.Application.Employees.List;

public sealed class ListEmployeesQueryHandler(
    IEmployeeRepository employeeRepository)
    : IRequestHandler<ListEmployeesQuery, IReadOnlyList<EmployeeListItem>>
{
    public async Task<IReadOnlyList<EmployeeListItem>> Handle(
        ListEmployeesQuery query,
        CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.ListAsync(cancellationToken);

        return employees
            .Select(e => new EmployeeListItem(
                e.Id.Value,
                e.FullName,
                e.RateHistory.Count > 0 ? e.RateHistory[^1].Rate : 0m))
            .ToList();
    }
}

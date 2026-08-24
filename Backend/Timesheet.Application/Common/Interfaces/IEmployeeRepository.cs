using Timesheet.Domain;
using Timesheet.Domain.Employees;

namespace Timesheet.Application.Common.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(EmployeeId id, CancellationToken ct);
    Task<IReadOnlyList<Employee>> ListAsync(CancellationToken ct);
    Task<long> ChangeRateAsync(EmployeeId id, DateOnly fromDate, decimal newRate, CancellationToken ct);
}

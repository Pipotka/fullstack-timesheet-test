using MediatR;

namespace Timesheet.Application.Employees.ChangeRate;

public sealed record ChangeEmployeeRateCommand(
    string EmployeeId,
    DateOnly FromDate,
    decimal NewRate) : IRequest<ChangeEmployeeRateResult>;

public sealed record ChangeEmployeeRateResult(long NewRateRevision);

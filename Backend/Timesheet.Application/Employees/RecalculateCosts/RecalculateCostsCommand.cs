using MediatR;

namespace Timesheet.Application.Employees.RecalculateCosts;

public sealed record RecalculateCostsCommand(
    string EmployeeId,
    long JobRevision) : IRequest;

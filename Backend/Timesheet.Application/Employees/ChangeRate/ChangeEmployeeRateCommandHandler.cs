using MediatR;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;

namespace Timesheet.Application.Employees.ChangeRate;

public sealed class ChangeEmployeeRateCommandHandler(
    IEmployeeRepository employeeRepository)
    : IRequestHandler<ChangeEmployeeRateCommand, ChangeEmployeeRateResult>
{
    public async Task<ChangeEmployeeRateResult> Handle(
        ChangeEmployeeRateCommand command,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(
            new EmployeeId(command.EmployeeId),
            cancellationToken);

        if (employee is null)
        {
            throw new BusinessException(
                ErrorCodes.EmployeeNotFound,
                ErrorMessages.EmployeeNotFound);
        }

        var newRevision = await employeeRepository.ChangeRateAsync(
            new EmployeeId(command.EmployeeId),
            command.FromDate,
            command.NewRate,
            cancellationToken);

        return new ChangeEmployeeRateResult(newRevision);
    }
}

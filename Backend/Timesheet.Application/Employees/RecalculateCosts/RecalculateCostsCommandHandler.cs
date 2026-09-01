using MediatR;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;

namespace Timesheet.Application.Employees.RecalculateCosts;

public sealed class RecalculateCostsCommandHandler(
    IEmployeeRepository employeeRepository,
    ITimeEntryRepository timeEntryRepository)
    : IRequestHandler<RecalculateCostsCommand>
{
    public async Task Handle(
        RecalculateCostsCommand command,
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

        var rateHistory = employee.RateHistory;

        for (var i = 0; i < rateHistory.Count; i++)
        {
            var from = rateHistory[i].From;
            var to = (i + 1 < rateHistory.Count)
                ? rateHistory[i + 1].From
                : DateOnly.MaxValue;
            var rate = rateHistory[i].Rate;

            var interval = new DateRange(from, to);

            await timeEntryRepository.UpdateCostsByIntervalAsync(
                new EmployeeId(command.EmployeeId),
                interval,
                rate,
                command.JobRevision,
                cancellationToken);
        }
    }
}

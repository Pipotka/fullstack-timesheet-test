using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;
using Timesheet.Domain.Common;

namespace Timesheet.Infrastructure.Maintenance;

public sealed class RateChangeService(
    IEmployeeRepository employeeRepository,
    ITimeEntryRepository timeEntryRepository)
{
    public async Task<long> ChangeRateAndRecalculateAsync(
        string employeeId,
        DateOnly fromDate,
        decimal newRate,
        CancellationToken ct)
    {
        var revision = await employeeRepository.ChangeRateAsync(
            new EmployeeId(employeeId),
            fromDate,
            newRate,
            ct);

        var employee = await employeeRepository.GetByIdAsync(
            new EmployeeId(employeeId),
            ct);

        if (employee is null)
        {
            return revision;
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
                new EmployeeId(employeeId),
                interval,
                rate,
                revision,
                ct);
        }

        return revision;
    }
}

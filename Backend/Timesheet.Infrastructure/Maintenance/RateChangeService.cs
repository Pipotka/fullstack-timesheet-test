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

        // Sort rate history by From date to ensure correct half-open intervals
        // This is necessary because AddToSet does not guarantee order
        var sortedHistory = employee.RateHistory
            .OrderBy(entry => entry.From)
            .ToList();

        for (var i = 0; i < sortedHistory.Count; i++)
        {
            var from = sortedHistory[i].From;
            var to = (i + 1 < sortedHistory.Count)
                ? sortedHistory[i + 1].From
                : DateOnly.MaxValue;
            var rate = sortedHistory[i].Rate;

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

using Timesheet.Domain;
using Timesheet.Domain.TimeEntries;
using Timesheet.Application.Common.Models;

namespace Timesheet.Application.Common.Interfaces;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(TimeEntryId id, CancellationToken ct);
    Task<(IReadOnlyList<TimeEntry> Items, int TotalCount)> ListAsync(TimeEntryFilter filter, CancellationToken ct);
    Task<decimal> SumHoursByEmployeeAndDateAsync(EmployeeId employeeId, DateOnly date, TimeEntryId? excludeId, CancellationToken ct);
    Task<(decimal TotalHours, decimal TotalCost)> SumByFilterAsync(TimeEntryFilter filter, CancellationToken ct);
    Task CreateAsync(TimeEntry entry, CancellationToken ct);
    Task<bool> UpdateAsync(TimeEntry entry, CancellationToken ct);
    Task<bool> DeleteAsync(TimeEntryId id, CancellationToken ct);
    Task UpdateCostsByIntervalAsync(EmployeeId employeeId, DateRange interval, decimal rate, long jobRevision, CancellationToken ct);
}

namespace Timesheet.Domain.Employees;

public sealed class Employee
{
    public EmployeeId Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public IReadOnlyList<RateHistoryEntry> RateHistory { get; init; } = [];
    public long RateRevision { get; init; }

    public decimal GetCurrentRate(DateOnly date)
    {
        if (RateHistory.Count == 0)
        {
            throw new BusinessException(
                "MISSING_RATE",
                "История ставок сотрудника пуста");
        }

        var applicable = RateHistory
            .Where(r => r.From <= date)
            .OrderByDescending(r => r.From)
            .FirstOrDefault();

        if (applicable is not null)
        {
            return applicable.Rate;
        }

        return RateHistory
            .OrderBy(r => r.From)
            .First()
            .Rate;
    }
}

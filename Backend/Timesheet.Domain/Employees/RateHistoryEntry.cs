namespace Timesheet.Domain.Employees;

public sealed record RateHistoryEntry
{
    public DateOnly From { get; init; }
    public decimal Rate { get; init; }
}

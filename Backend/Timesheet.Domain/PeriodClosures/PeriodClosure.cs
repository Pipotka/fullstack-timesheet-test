namespace Timesheet.Domain.PeriodClosures;

public sealed class PeriodClosure
{
    public int Year { get; init; }
    public int Month { get; init; }
    public bool IsClosed { get; init; }
}

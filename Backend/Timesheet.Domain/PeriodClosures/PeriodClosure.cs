namespace Timesheet.Domain.PeriodClosures;

public sealed class PeriodClosure
{
    public int Year { get; init; }
    public int Month { get; init; }
    public bool IsClosed { get; init; }

    public void ValidateInvariants()
    {
        if (Year <= 0 || Month < 1 || Month > 12)
        {
            throw new BusinessException(
                "INVALID_PERIOD",
                "Некорректный период: год должен быть больше 0, месяц от 1 до 12");
        }
    }
}

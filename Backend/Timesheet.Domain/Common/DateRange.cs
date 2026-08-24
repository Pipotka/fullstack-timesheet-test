namespace Timesheet.Domain;

public readonly record struct DateRange
{
    public DateOnly From { get; }
    public DateOnly To { get; }

    public DateRange(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentException(
                $"Начало диапазона ({from:yyyy-MM-dd}) не может быть позже конца ({to:yyyy-MM-dd}).",
                nameof(from));
        }

        From = from;
        To = to;
    }
}

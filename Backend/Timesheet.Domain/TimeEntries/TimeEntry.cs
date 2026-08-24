using Timesheet.Domain.Common;

namespace Timesheet.Domain.TimeEntries;

public sealed class TimeEntry
{
    public TimeEntryId Id { get; init; }
    public EmployeeId EmployeeId { get; init; }
    public ProjectId ProjectId { get; init; }
    public DateOnly Date { get; init; }
    public decimal Hours { get; init; }
    public string Comment { get; init; } = string.Empty;
    public decimal AppliedRate { get; init; }
    public decimal Cost { get; init; }
    public long RateRevision { get; init; }
    public long Version { get; init; }

    public static decimal CalculateCost(decimal hours, decimal rate)
        => Math.Round(hours * rate, 2, MidpointRounding.AwayFromZero);

    public void ValidateInvariants()
    {
        if (Hours <= 0 || Hours > 24)
        {
            throw new BusinessException(
                DomainErrorCodes.InvalidHours,
                DomainErrorMessages.InvalidHours);
        }

        var remainder = Hours % 0.5m;
        if (remainder != 0)
        {
            throw new BusinessException(
                DomainErrorCodes.InvalidHours,
                DomainErrorMessages.InvalidHours);
        }

        if (AppliedRate < 0)
        {
            throw new BusinessException(
                DomainErrorCodes.InvalidAppliedRate,
                DomainErrorMessages.InvalidAppliedRate);
        }
    }
}

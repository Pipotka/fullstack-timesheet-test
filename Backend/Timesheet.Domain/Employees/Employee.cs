using Timesheet.Domain.Common;

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
                DomainErrorCodes.MissingRate,
                DomainErrorMessages.MissingRate);
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

    public void ValidateInvariants()
    {
        if (RateHistory.Count == 0)
        {
            throw new BusinessException(
                DomainErrorCodes.MissingRate,
                DomainErrorMessages.MissingRate);
        }

        var dates = new HashSet<DateOnly>();
        DateOnly? previous = null;

        foreach (var entry in RateHistory)
        {
            if (!dates.Add(entry.From))
            {
                throw new BusinessException(
                    DomainErrorCodes.DuplicateRateDate,
                    $"{DomainErrorMessages.DuplicateRateDate}: {entry.From:yyyy-MM-dd}");
            }

            if (previous.HasValue && entry.From < previous.Value)
            {
                throw new BusinessException(
                    DomainErrorCodes.InvalidRateHistory,
                    DomainErrorMessages.InvalidRateHistory);
            }

            previous = entry.From;
        }
    }
}

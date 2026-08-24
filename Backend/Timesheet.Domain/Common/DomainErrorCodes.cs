namespace Timesheet.Domain.Common;

public static class DomainErrorCodes
{
    public const string MissingRate = "MISSING_RATE";
    public const string DuplicateRateDate = "DUPLICATE_RATE_DATE";
    public const string InvalidRateHistory = "INVALID_RATE_HISTORY";
    public const string InvalidBudget = "INVALID_BUDGET";
    public const string InvalidDateRange = "INVALID_DATE_RANGE";
    public const string InvalidHours = "INVALID_HOURS";
    public const string InvalidAppliedRate = "INVALID_APPLIED_RATE";
    public const string InvalidPeriod = "INVALID_PERIOD";
}

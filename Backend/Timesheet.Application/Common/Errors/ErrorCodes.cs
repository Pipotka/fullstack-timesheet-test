namespace Timesheet.Application.Common.Errors;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string DailyLimitExceeded = "DAILY_LIMIT_EXCEEDED";
    public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";
    public const string ProjectNotFound = "PROJECT_NOT_FOUND";
    public const string TimeEntryNotFound = "TIME_ENTRY_NOT_FOUND";
    public const string PeriodClosed = "PERIOD_CLOSED";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";

    public const string MissingRate = "MISSING_RATE";
    public const string DuplicateRateDate = "DUPLICATE_RATE_DATE";
    public const string InvalidRateHistory = "INVALID_RATE_HISTORY";
    public const string InvalidBudget = "INVALID_BUDGET";
    public const string InvalidDateRange = "INVALID_DATE_RANGE";
    public const string InvalidHours = "INVALID_HOURS";
    public const string InvalidAppliedRate = "INVALID_APPLIED_RATE";
    public const string InvalidPeriod = "INVALID_PERIOD";
}

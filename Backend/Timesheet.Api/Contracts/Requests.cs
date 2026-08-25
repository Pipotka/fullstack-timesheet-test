namespace Timesheet.Api.Contracts;

public sealed record CreateTimeEntryRequest(
    string EmployeeId,
    string ProjectId,
    string Date,
    decimal Hours,
    string Comment);

public sealed record UpdateTimeEntryRequest(
    long Version,
    decimal Hours,
    string Comment);

public sealed record PeriodRequest(int Year, int Month);

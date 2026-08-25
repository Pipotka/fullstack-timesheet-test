namespace Timesheet.Infrastructure.MongoDb.Documents;

public sealed class TimeEntryDocument
{
    public string Id { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Hours { get; set; }
    public string Comment { get; set; } = string.Empty;
    public decimal AppliedRate { get; set; }
    public decimal Cost { get; set; }
    public long RateRevision { get; set; }
    public long Version { get; set; }
}

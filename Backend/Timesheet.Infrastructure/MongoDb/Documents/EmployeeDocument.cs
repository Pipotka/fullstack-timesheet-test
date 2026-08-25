namespace Timesheet.Infrastructure.MongoDb.Documents;

public sealed class EmployeeDocument
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<RateHistoryEntryDocument> RateHistory { get; set; } = [];
    public long RateRevision { get; set; }
}

public sealed class RateHistoryEntryDocument
{
    public DateOnly From { get; set; }
    public decimal Rate { get; set; }
}

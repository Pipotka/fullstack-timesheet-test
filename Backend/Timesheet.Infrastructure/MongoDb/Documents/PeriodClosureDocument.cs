namespace Timesheet.Infrastructure.MongoDb.Documents;

public sealed class PeriodClosureDocument
{
    public int Year { get; set; }
    public int Month { get; set; }
    public bool IsClosed { get; set; }
}

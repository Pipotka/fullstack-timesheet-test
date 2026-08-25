using Timesheet.Domain.PeriodClosures;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.DocumentMapping;

public static class PeriodClosureMapper
{
    public static PeriodClosure ToDomain(PeriodClosureDocument document)
    {
        return new PeriodClosure
        {
            Year = document.Year,
            Month = document.Month,
            IsClosed = document.IsClosed
        };
    }

    public static PeriodClosureDocument ToDocument(PeriodClosure domain)
    {
        return new PeriodClosureDocument
        {
            Year = domain.Year,
            Month = domain.Month,
            IsClosed = domain.IsClosed
        };
    }
}

using Timesheet.Domain;
using Timesheet.Domain.Employees;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.DocumentMapping;

public static class EmployeeMapper
{
    public static Employee ToDomain(EmployeeDocument document)
    {
        return new Employee
        {
            Id = new EmployeeId(document.Id),
            FullName = document.FullName,
            RateHistory = document.RateHistory
                .Select(r => new RateHistoryEntry
                {
                    From = r.From,
                    Rate = r.Rate
                })
                .ToList()
                .AsReadOnly(),
            RateRevision = document.RateRevision
        };
    }

    public static EmployeeDocument ToDocument(Employee domain)
    {
        return new EmployeeDocument
        {
            Id = domain.Id.Value,
            FullName = domain.FullName,
            RateHistory = domain.RateHistory
                .Select(r => new RateHistoryEntryDocument
                {
                    From = r.From,
                    Rate = r.Rate
                })
                .ToList(),
            RateRevision = domain.RateRevision
        };
    }
}

using Timesheet.Domain;
using Timesheet.Domain.TimeEntries;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.DocumentMapping;

public static class TimeEntryMapper
{
    public static TimeEntry ToDomain(TimeEntryDocument document)
    {
        return new TimeEntry
        {
            Id = new TimeEntryId(document.Id),
            EmployeeId = new EmployeeId(document.EmployeeId),
            ProjectId = new ProjectId(document.ProjectId),
            Date = document.Date,
            Hours = document.Hours,
            Comment = document.Comment,
            AppliedRate = document.AppliedRate,
            Cost = document.Cost,
            RateRevision = document.RateRevision,
            Version = document.Version
        };
    }

    public static TimeEntryDocument ToDocument(TimeEntry domain)
    {
        return new TimeEntryDocument
        {
            Id = domain.Id.Value,
            EmployeeId = domain.EmployeeId.Value,
            ProjectId = domain.ProjectId.Value,
            Date = domain.Date,
            Hours = domain.Hours,
            Comment = domain.Comment,
            AppliedRate = domain.AppliedRate,
            Cost = domain.Cost,
            RateRevision = domain.RateRevision,
            Version = domain.Version
        };
    }
}

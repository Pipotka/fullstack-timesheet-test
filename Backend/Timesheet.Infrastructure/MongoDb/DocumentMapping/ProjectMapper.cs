using Timesheet.Domain;
using Timesheet.Domain.Projects;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.DocumentMapping;

public static class ProjectMapper
{
    public static Project ToDomain(ProjectDocument document)
    {
        return new Project
        {
            Id = new ProjectId(document.Id),
            Code = document.Code,
            Name = document.Name,
            Budget = document.Budget,
            StartDate = document.StartDate,
            EndDate = document.EndDate
        };
    }

    public static ProjectDocument ToDocument(Project domain)
    {
        return new ProjectDocument
        {
            Id = domain.Id.Value,
            Code = domain.Code,
            Name = domain.Name,
            Budget = domain.Budget,
            StartDate = domain.StartDate,
            EndDate = domain.EndDate
        };
    }
}

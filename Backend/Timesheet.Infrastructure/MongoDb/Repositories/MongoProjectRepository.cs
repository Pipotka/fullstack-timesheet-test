using MongoDB.Bson;
using MongoDB.Driver;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Common.Models;
using Timesheet.Domain;
using Timesheet.Domain.Projects;
using Timesheet.Infrastructure.MongoDb.DocumentMapping;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.Repositories;

public sealed class MongoProjectRepository : IProjectRepository
{
    private readonly IMongoCollection<ProjectDocument> _projects;
    private readonly IMongoCollection<TimeEntryDocument> _timeEntries;

    public MongoProjectRepository(IMongoDatabase database)
    {
        _projects = database.GetCollection<ProjectDocument>("projects");
        _timeEntries = database.GetCollection<TimeEntryDocument>("time_entries");
    }

    public async Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct)
    {
        var filter = Builders<ProjectDocument>.Filter.Eq(x => x.Id, id.Value);
        var document = await _projects.Find(filter).FirstOrDefaultAsync(ct);
        return document is null ? null : ProjectMapper.ToDomain(document);
    }

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct)
    {
        var documents = await _projects.Find(Builders<ProjectDocument>.Filter.Empty)
            .Sort(Builders<ProjectDocument>.Sort.Ascending(x => x.Code))
            .ToListAsync(ct);

        return documents.Select(ProjectMapper.ToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<ProjectReportResult>> GetReportsByPeriodAsync(
        int year,
        int month,
        CancellationToken ct)
    {
        var fromDate = new DateOnly(year, month, 1);
        var toDate = fromDate.AddMonths(1);

        var filter = Builders<TimeEntryDocument>.Filter.And(
            Builders<TimeEntryDocument>.Filter.Gte(x => x.Date, fromDate),
            Builders<TimeEntryDocument>.Filter.Lt(x => x.Date, toDate));

        var aggregation = await _timeEntries.Aggregate()
            .Match(filter)
            .Group(new BsonDocument
            {
                { "_id", "$projectId" },
                { "TotalHours", new BsonDocument("$sum", "$hours") },
                { "TotalCost", new BsonDocument("$sum", "$cost") }
            })
            .ToListAsync(ct);

        if (aggregation.Count == 0)
        {
            return Array.Empty<ProjectReportResult>();
        }

        var projectIds = aggregation.Select(a => a["_id"].AsString).ToList();
        var projectFilter = Builders<ProjectDocument>.Filter.In(x => x.Id, projectIds);
        var projects = await _projects.Find(projectFilter).ToListAsync(ct);
        var projectLookup = projects.ToDictionary(p => p.Id, p => p);

        var results = aggregation
            .Select(a =>
            {
                var projectId = a["_id"].AsString;
                if (projectLookup.TryGetValue(projectId, out var project))
                {
                    return new ProjectReportResult(
                        new ProjectId(projectId),
                        project.Code,
                        project.Name,
                        project.Budget,
                        a["TotalHours"].ToDecimal(),
                        a["TotalCost"].ToDecimal());
                }

                return null;
            })
            .Where(r => r is not null)
            .ToList()
            .AsReadOnly();

        return results!;
    }
}

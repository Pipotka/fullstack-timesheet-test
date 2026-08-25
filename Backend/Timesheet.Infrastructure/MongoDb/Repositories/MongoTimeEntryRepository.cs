using MongoDB.Bson;
using MongoDB.Driver;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Common.Models;
using Timesheet.Domain;
using Timesheet.Domain.TimeEntries;
using Timesheet.Infrastructure.MongoDb.DocumentMapping;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.Repositories;

public sealed class MongoTimeEntryRepository : ITimeEntryRepository
{
    private readonly IMongoCollection<TimeEntryDocument> _collection;

    public MongoTimeEntryRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<TimeEntryDocument>("time_entries");
    }

    public async Task<TimeEntry?> GetByIdAsync(TimeEntryId id, CancellationToken ct)
    {
        var filter = Builders<TimeEntryDocument>.Filter.Eq(x => x.Id, id.Value);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document is null ? null : TimeEntryMapper.ToDomain(document);
    }

    public async Task<(IReadOnlyList<TimeEntry> Items, int TotalCount)> ListAsync(
        TimeEntryFilter filter,
        CancellationToken ct)
    {
        var mongoFilter = BuildFilter(filter);

        var countTask = _collection.CountDocumentsAsync(mongoFilter, cancellationToken: ct);

        var findTask = _collection.Find(mongoFilter)
            .Sort(Builders<TimeEntryDocument>.Sort.Descending(x => x.Date).Ascending(x => x.Id))
            .Skip((filter.Page - 1) * filter.PageSize)
            .Limit(filter.PageSize)
            .ToListAsync(ct);

        await Task.WhenAll(countTask, findTask);

        var totalCount = (int)await countTask;
        var documents = await findTask;
        var items = documents.Select(TimeEntryMapper.ToDomain).ToList().AsReadOnly();

        return (items, totalCount);
    }

    public async Task<decimal> SumHoursByEmployeeAndDateAsync(
        EmployeeId employeeId,
        DateOnly date,
        TimeEntryId? excludeId,
        CancellationToken ct)
    {
        var filter = Builders<TimeEntryDocument>.Filter.And(
            Builders<TimeEntryDocument>.Filter.Eq(x => x.EmployeeId, employeeId.Value),
            Builders<TimeEntryDocument>.Filter.Eq(x => x.Date, date));

        if (excludeId.HasValue)
        {
            filter &= Builders<TimeEntryDocument>.Filter.Ne(x => x.Id, excludeId.Value.Value);
        }

        var result = await _collection.Aggregate()
            .Match(filter)
            .Group(new BsonDocument
            {
                { "_id", 1 },
                { "Total", new BsonDocument("$sum", "$hours") }
            })
            .FirstOrDefaultAsync(ct);

        return result?.GetValue("Total", 0)?.ToDecimal() ?? 0m;
    }

    public async Task<(decimal TotalHours, decimal TotalCost)> SumByFilterAsync(
        TimeEntryFilter filter,
        CancellationToken ct)
    {
        var mongoFilter = BuildFilter(filter);

        var result = await _collection.Aggregate()
            .Match(mongoFilter)
            .Group(new BsonDocument
            {
                { "_id", 1 },
                { "TotalHours", new BsonDocument("$sum", "$hours") },
                { "TotalCost", new BsonDocument("$sum", "$cost") }
            })
            .FirstOrDefaultAsync(ct);

        if (result is null)
        {
            return (0m, 0m);
        }

        var totalHours = result.GetValue("TotalHours", 0).ToDecimal();
        var totalCost = result.GetValue("TotalCost", 0).ToDecimal();

        return (totalHours, totalCost);
    }

    public async Task CreateAsync(TimeEntry entry, CancellationToken ct)
    {
        var document = TimeEntryMapper.ToDocument(entry);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task<bool> UpdateAsync(TimeEntry entry, CancellationToken ct)
    {
        var document = TimeEntryMapper.ToDocument(entry);
        var filter = Builders<TimeEntryDocument>.Filter.And(
            Builders<TimeEntryDocument>.Filter.Eq(x => x.Id, document.Id),
            Builders<TimeEntryDocument>.Filter.Eq(x => x.Version, entry.Version - 1));

        var result = await _collection.ReplaceOneAsync(filter, document, cancellationToken: ct);
        return result.ModifiedCount == 1;
    }

    public async Task<bool> DeleteAsync(TimeEntryId id, CancellationToken ct)
    {
        var filter = Builders<TimeEntryDocument>.Filter.Eq(x => x.Id, id.Value);
        var result = await _collection.DeleteOneAsync(filter, ct);
        return result.DeletedCount == 1;
    }

    public async Task UpdateCostsByIntervalAsync(
        EmployeeId employeeId,
        DateRange interval,
        decimal rate,
        long jobRevision,
        CancellationToken ct)
    {
        var filter = Builders<TimeEntryDocument>.Filter.And(
            Builders<TimeEntryDocument>.Filter.Eq(x => x.EmployeeId, employeeId.Value),
            Builders<TimeEntryDocument>.Filter.Gte(x => x.Date, interval.From),
            Builders<TimeEntryDocument>.Filter.Lt(x => x.Date, interval.To),
            Builders<TimeEntryDocument>.Filter.Lt(x => x.RateRevision, jobRevision));

        var pipelineStages = new BsonDocument[]
        {
            new BsonDocument("$set", new BsonDocument
            {
                { "appliedRate", rate },
                { "cost", new BsonDocument("$round", new BsonArray
                {
                    new BsonDocument("$multiply", new BsonArray { "$hours", rate }),
                    2
                }) },
                { "rateRevision", jobRevision }
            })
        };

        var pipeline = PipelineDefinition<TimeEntryDocument, TimeEntryDocument>.Create(pipelineStages);
        await _collection.UpdateManyAsync(filter, pipeline, options: null, cancellationToken: ct);
    }

    private static FilterDefinition<TimeEntryDocument> BuildFilter(TimeEntryFilter filter)
    {
        var filters = new List<FilterDefinition<TimeEntryDocument>>();

        if (filter.EmployeeId.HasValue)
        {
            filters.Add(Builders<TimeEntryDocument>.Filter.Eq(x => x.EmployeeId, filter.EmployeeId.Value.Value));
        }

        if (filter.ProjectId.HasValue)
        {
            filters.Add(Builders<TimeEntryDocument>.Filter.Eq(x => x.ProjectId, filter.ProjectId.Value.Value));
        }

        if (filter.FromDate.HasValue)
        {
            filters.Add(Builders<TimeEntryDocument>.Filter.Gte(x => x.Date, filter.FromDate.Value));
        }

        if (filter.ToDate.HasValue)
        {
            filters.Add(Builders<TimeEntryDocument>.Filter.Lte(x => x.Date, filter.ToDate.Value));
        }

        return filters.Count > 0
            ? Builders<TimeEntryDocument>.Filter.And(filters)
            : Builders<TimeEntryDocument>.Filter.Empty;
    }
}

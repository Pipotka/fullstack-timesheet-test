using MongoDB.Driver;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.Indexes;

public sealed class IndexCreator
{
    private readonly IMongoDatabase _database;

    public IndexCreator(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task CreateIndexesAsync(CancellationToken ct)
    {
        await CreateTimeEntryIndexesAsync(ct);
        await CreateProjectIndexesAsync(ct);
        await CreatePeriodClosureIndexesAsync(ct);
        await CreateEmployeeIndexesAsync(ct);
    }

    private async Task CreateTimeEntryIndexesAsync(CancellationToken ct)
    {
        var collection = _database.GetCollection<TimeEntryDocument>("time_entries");

        var indexes = new[]
        {
            new CreateIndexModel<TimeEntryDocument>(
                Builders<TimeEntryDocument>.IndexKeys.Ascending(x => x.EmployeeId).Ascending(x => x.Date),
                new CreateIndexOptions { Name = "idx_time_entries_employee_date" }),

            new CreateIndexModel<TimeEntryDocument>(
                Builders<TimeEntryDocument>.IndexKeys.Ascending(x => x.ProjectId).Ascending(x => x.Date),
                new CreateIndexOptions { Name = "idx_time_entries_project_date" }),

            new CreateIndexModel<TimeEntryDocument>(
                Builders<TimeEntryDocument>.IndexKeys.Ascending(x => x.Date).Ascending(x => x.Id),
                new CreateIndexOptions { Name = "idx_time_entries_date_id" })
        };

        await collection.Indexes.CreateManyAsync(indexes, ct);
    }

    private async Task CreateProjectIndexesAsync(CancellationToken ct)
    {
        var collection = _database.GetCollection<ProjectDocument>("projects");

        var index = new CreateIndexModel<ProjectDocument>(
            Builders<ProjectDocument>.IndexKeys.Ascending(x => x.Code),
            new CreateIndexOptions
            {
                Name = "idx_projects_code_unique",
                Unique = true
            });

        await collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
    }

    private async Task CreatePeriodClosureIndexesAsync(CancellationToken ct)
    {
        var collection = _database.GetCollection<PeriodClosureDocument>("period_closures");

        var index = new CreateIndexModel<PeriodClosureDocument>(
            Builders<PeriodClosureDocument>.IndexKeys.Ascending(x => x.Year).Ascending(x => x.Month),
            new CreateIndexOptions
            {
                Name = "idx_period_closures_year_month_unique",
                Unique = true
            });

        await collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
    }

    private async Task CreateEmployeeIndexesAsync(CancellationToken ct)
    {
        var collection = _database.GetCollection<EmployeeDocument>("employees");

        var index = new CreateIndexModel<EmployeeDocument>(
            Builders<EmployeeDocument>.IndexKeys.Ascending(x => x.FullName),
            new CreateIndexOptions { Name = "idx_employees_fullname" });

        await collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
    }
}

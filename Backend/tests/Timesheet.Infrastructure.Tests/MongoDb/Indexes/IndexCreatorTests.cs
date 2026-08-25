using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;
using Timesheet.Infrastructure.MongoDb.Documents;
using Timesheet.Infrastructure.MongoDb.Indexes;

namespace Timesheet.Infrastructure.Tests.MongoDb.Indexes;

public sealed class IndexCreatorTests
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<TimeEntryDocument> _timeEntries;
    private readonly IMongoCollection<ProjectDocument> _projects;
    private readonly IMongoCollection<PeriodClosureDocument> _periodClosures;
    private readonly IMongoCollection<EmployeeDocument> _employees;
    private readonly IndexCreator _indexCreator;

    public IndexCreatorTests()
    {
        _database = Substitute.For<IMongoDatabase>();
        _timeEntries = Substitute.For<IMongoCollection<TimeEntryDocument>>();
        _projects = Substitute.For<IMongoCollection<ProjectDocument>>();
        _periodClosures = Substitute.For<IMongoCollection<PeriodClosureDocument>>();
        _employees = Substitute.For<IMongoCollection<EmployeeDocument>>();

        _database.GetCollection<TimeEntryDocument>("time_entries").Returns(_timeEntries);
        _database.GetCollection<ProjectDocument>("projects").Returns(_projects);
        _database.GetCollection<PeriodClosureDocument>("period_closures").Returns(_periodClosures);
        _database.GetCollection<EmployeeDocument>("employees").Returns(_employees);

        var mockIndexes = Substitute.For<IMongoIndexManager<TimeEntryDocument>>();
        _timeEntries.Indexes.Returns(mockIndexes);

        var mockProjectIndexes = Substitute.For<IMongoIndexManager<ProjectDocument>>();
        _projects.Indexes.Returns(mockProjectIndexes);

        var mockPeriodIndexes = Substitute.For<IMongoIndexManager<PeriodClosureDocument>>();
        _periodClosures.Indexes.Returns(mockPeriodIndexes);

        var mockEmployeeIndexes = Substitute.For<IMongoIndexManager<EmployeeDocument>>();
        _employees.Indexes.Returns(mockEmployeeIndexes);

        _indexCreator = new IndexCreator(_database);
    }

    [Fact]
    public async Task CreateIndexesAsync_CreatesTimeEntryIndexes()
    {
        await _indexCreator.CreateIndexesAsync(CancellationToken.None);

        await _timeEntries.Indexes.Received(1).CreateManyAsync(
            Arg.Any<IEnumerable<CreateIndexModel<TimeEntryDocument>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateIndexesAsync_CreatesProjectIndex()
    {
        await _indexCreator.CreateIndexesAsync(CancellationToken.None);

        await _projects.Indexes.Received(1).CreateOneAsync(
            Arg.Any<CreateIndexModel<ProjectDocument>>(),
            Arg.Any<CreateOneIndexOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateIndexesAsync_CreatesPeriodClosureIndex()
    {
        await _indexCreator.CreateIndexesAsync(CancellationToken.None);

        await _periodClosures.Indexes.Received(1).CreateOneAsync(
            Arg.Any<CreateIndexModel<PeriodClosureDocument>>(),
            Arg.Any<CreateOneIndexOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateIndexesAsync_CreatesEmployeeIndex()
    {
        await _indexCreator.CreateIndexesAsync(CancellationToken.None);

        await _employees.Indexes.Received(1).CreateOneAsync(
            Arg.Any<CreateIndexModel<EmployeeDocument>>(),
            Arg.Any<CreateOneIndexOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateIndexesAsync_IsIdempotent()
    {
        var act = async () =>
        {
            await _indexCreator.CreateIndexesAsync(CancellationToken.None);
            await _indexCreator.CreateIndexesAsync(CancellationToken.None);
            await _indexCreator.CreateIndexesAsync(CancellationToken.None);
        };

        await act.Should().NotThrowAsync();
    }
}

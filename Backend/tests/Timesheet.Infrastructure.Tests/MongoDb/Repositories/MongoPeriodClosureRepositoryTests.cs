using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;
using Timesheet.Infrastructure.MongoDb.Documents;
using Timesheet.Infrastructure.MongoDb.Repositories;

namespace Timesheet.Infrastructure.Tests.MongoDb.Repositories;

public sealed class MongoPeriodClosureRepositoryTests
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<PeriodClosureDocument> _collection;
    private readonly MongoPeriodClosureRepository _repository;

    public MongoPeriodClosureRepositoryTests()
    {
        _database = Substitute.For<IMongoDatabase>();
        _collection = Substitute.For<IMongoCollection<PeriodClosureDocument>>();
        _database.GetCollection<PeriodClosureDocument>("period_closures").Returns(_collection);
        _repository = new MongoPeriodClosureRepository(_database);
    }

    [Fact]
    public void Constructor_UsesCorrectCollectionName()
    {
        _database.Received(1).GetCollection<PeriodClosureDocument>("period_closures");
    }

    [Fact]
    public async Task SetClosedAsync_InsertsOrUpdatesDocument()
    {
        var replaceResult = Substitute.For<ReplaceOneResult>();
        replaceResult.ModifiedCount.Returns(1);
        _collection.ReplaceOneAsync(
            Arg.Any<FilterDefinition<PeriodClosureDocument>>(),
            Arg.Any<PeriodClosureDocument>(),
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>()).Returns(replaceResult);

        await _repository.SetClosedAsync(2024, 8, true, CancellationToken.None);

        await _collection.Received(1).ReplaceOneAsync(
            Arg.Any<FilterDefinition<PeriodClosureDocument>>(),
            Arg.Is<PeriodClosureDocument>(d => d.Year == 2024 && d.Month == 8 && d.IsClosed),
            Arg.Is<ReplaceOptions>(o => o.IsUpsert),
            Arg.Any<CancellationToken>());
    }
}

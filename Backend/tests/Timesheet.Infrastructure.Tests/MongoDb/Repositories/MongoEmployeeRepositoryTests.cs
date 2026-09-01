using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;
using Timesheet.Domain;
using Timesheet.Infrastructure.MongoDb.Documents;
using Timesheet.Infrastructure.MongoDb.Repositories;

namespace Timesheet.Infrastructure.Tests.MongoDb.Repositories;

public sealed class MongoEmployeeRepositoryTests
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<EmployeeDocument> _collection;
    private readonly MongoEmployeeRepository _repository;

    public MongoEmployeeRepositoryTests()
    {
        _database = Substitute.For<IMongoDatabase>();
        _collection = Substitute.For<IMongoCollection<EmployeeDocument>>();
        _database.GetCollection<EmployeeDocument>("employees").Returns(_collection);
        _repository = new MongoEmployeeRepository(_database);
    }

    [Fact]
    public void Constructor_UsesCorrectCollectionName()
    {
        _database.Received(1).GetCollection<EmployeeDocument>("employees");
    }

    [Fact]
    public async Task ChangeRateAsync_WhenEmployeeExists_ReturnsNewRevision()
    {
        var updatedDocument = new EmployeeDocument
        {
            Id = "emp-001",
            FullName = "John Doe",
            RateHistory =
            [
                new RateHistoryEntryDocument { From = new DateOnly(2024, 1, 1), Rate = 100.00m },
                new RateHistoryEntryDocument { From = new DateOnly(2025, 1, 1), Rate = 150.00m }
            ],
            RateRevision = 2
        };

        _collection.FindOneAndUpdateAsync(
            Arg.Any<FilterDefinition<EmployeeDocument>>(),
            Arg.Any<UpdateDefinition<EmployeeDocument>>(),
            Arg.Any<FindOneAndUpdateOptions<EmployeeDocument>>(),
            Arg.Any<CancellationToken>()).Returns(updatedDocument);

        var result = await _repository.ChangeRateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2025, 1, 1),
            150.00m,
            CancellationToken.None);

        result.Should().Be(2);
    }
}

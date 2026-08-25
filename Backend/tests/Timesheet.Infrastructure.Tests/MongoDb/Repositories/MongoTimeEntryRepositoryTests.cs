using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;
using Timesheet.Domain;
using Timesheet.Domain.TimeEntries;
using Timesheet.Infrastructure.MongoDb.Documents;
using Timesheet.Infrastructure.MongoDb.Repositories;

namespace Timesheet.Infrastructure.Tests.MongoDb.Repositories;

public sealed class MongoTimeEntryRepositoryTests
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<TimeEntryDocument> _collection;
    private readonly MongoTimeEntryRepository _repository;

    public MongoTimeEntryRepositoryTests()
    {
        _database = Substitute.For<IMongoDatabase>();
        _collection = Substitute.For<IMongoCollection<TimeEntryDocument>>();
        _database.GetCollection<TimeEntryDocument>("time_entries").Returns(_collection);
        _repository = new MongoTimeEntryRepository(_database);
    }

    [Fact]
    public void Constructor_UsesCorrectCollectionName()
    {
        _database.Received(1).GetCollection<TimeEntryDocument>("time_entries");
    }

    [Fact]
    public async Task CreateAsync_InsertsDocument()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("te-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2024, 8, 25),
            Hours = 8.0m,
            Comment = "Test",
            AppliedRate = 100.00m,
            Cost = 800.00m,
            RateRevision = 1,
            Version = 1
        };

        await _repository.CreateAsync(entry, CancellationToken.None);

        await _collection.Received(1).InsertOneAsync(
            Arg.Is<TimeEntryDocument>(d => d.Id == "te-001"),
            Arg.Any<InsertOneOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenVersionMatches_ReturnsTrue()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("te-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2024, 8, 25),
            Hours = 8.0m,
            Comment = "Updated",
            AppliedRate = 100.00m,
            Cost = 800.00m,
            RateRevision = 1,
            Version = 2
        };

        var replaceResult = Substitute.For<ReplaceOneResult>();
        replaceResult.ModifiedCount.Returns(1);
        _collection.ReplaceOneAsync(
            Arg.Any<FilterDefinition<TimeEntryDocument>>(),
            Arg.Any<TimeEntryDocument>(),
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>()).Returns(replaceResult);

        var result = await _repository.UpdateAsync(entry, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WhenVersionMismatch_ReturnsFalse()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("te-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2024, 8, 25),
            Hours = 8.0m,
            Comment = "Updated",
            AppliedRate = 100.00m,
            Cost = 800.00m,
            RateRevision = 1,
            Version = 2
        };

        var replaceResult = Substitute.For<ReplaceOneResult>();
        replaceResult.ModifiedCount.Returns(0);
        _collection.ReplaceOneAsync(
            Arg.Any<FilterDefinition<TimeEntryDocument>>(),
            Arg.Any<TimeEntryDocument>(),
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>()).Returns(replaceResult);

        var result = await _repository.UpdateAsync(entry, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenDocumentExists_ReturnsTrue()
    {
        var deleteResult = Substitute.For<DeleteResult>();
        deleteResult.DeletedCount.Returns(1);
        _collection.DeleteOneAsync(
            Arg.Any<FilterDefinition<TimeEntryDocument>>(),
            Arg.Any<CancellationToken>()).Returns(deleteResult);

        var result = await _repository.DeleteAsync(new TimeEntryId("te-001"), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenDocumentNotFound_ReturnsFalse()
    {
        var deleteResult = Substitute.For<DeleteResult>();
        deleteResult.DeletedCount.Returns(0);
        _collection.DeleteOneAsync(
            Arg.Any<FilterDefinition<TimeEntryDocument>>(),
            Arg.Any<CancellationToken>()).Returns(deleteResult);

        var result = await _repository.DeleteAsync(new TimeEntryId("nonexistent"), CancellationToken.None);

        result.Should().BeFalse();
    }
}

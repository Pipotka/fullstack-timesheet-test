using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;
using Timesheet.Domain;
using Timesheet.Domain.Projects;
using Timesheet.Infrastructure.MongoDb.Documents;
using Timesheet.Infrastructure.MongoDb.Repositories;

namespace Timesheet.Infrastructure.Tests.MongoDb.Repositories;

public sealed class MongoProjectRepositoryTests
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ProjectDocument> _projects;
    private readonly IMongoCollection<TimeEntryDocument> _timeEntries;
    private readonly MongoProjectRepository _repository;

    public MongoProjectRepositoryTests()
    {
        _database = Substitute.For<IMongoDatabase>();
        _projects = Substitute.For<IMongoCollection<ProjectDocument>>();
        _timeEntries = Substitute.For<IMongoCollection<TimeEntryDocument>>();
        _database.GetCollection<ProjectDocument>("projects").Returns(_projects);
        _database.GetCollection<TimeEntryDocument>("time_entries").Returns(_timeEntries);
        _repository = new MongoProjectRepository(_database);
    }

    [Fact]
    public void Constructor_UsesCorrectCollectionNames()
    {
        _database.Received(1).GetCollection<ProjectDocument>("projects");
        _database.Received(1).GetCollection<TimeEntryDocument>("time_entries");
    }
}

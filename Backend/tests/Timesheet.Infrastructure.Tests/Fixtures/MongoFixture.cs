using MongoDB.Driver;

namespace Timesheet.Infrastructure.Tests.Fixtures;

public sealed class MongoFixture : IDisposable
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly string _databaseName;

    public MongoFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
            ?? "mongodb://localhost:27017";

        _databaseName = $"timesheet_test_{Guid.NewGuid():N}";
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase(_databaseName);
    }

    public IMongoDatabase Database => _database;

    public void Dispose()
    {
        try
        {
            _client.DropDatabase(_databaseName);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}

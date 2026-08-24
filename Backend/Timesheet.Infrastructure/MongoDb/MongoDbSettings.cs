namespace Timesheet.Infrastructure.MongoDb;

public sealed record MongoDbSettings
{
    public string ConnectionString { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = string.Empty;
}

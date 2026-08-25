using MongoDB.Bson;
using MongoDB.Driver;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;
using Timesheet.Domain.Employees;
using Timesheet.Infrastructure.MongoDb.DocumentMapping;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.Repositories;

public sealed class MongoEmployeeRepository : IEmployeeRepository
{
    private readonly IMongoCollection<EmployeeDocument> _collection;

    public MongoEmployeeRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<EmployeeDocument>("employees");
    }

    public async Task<Employee?> GetByIdAsync(EmployeeId id, CancellationToken ct)
    {
        var filter = Builders<EmployeeDocument>.Filter.Eq(x => x.Id, id.Value);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document is null ? null : EmployeeMapper.ToDomain(document);
    }

    public async Task<IReadOnlyList<Employee>> ListAsync(CancellationToken ct)
    {
        var documents = await _collection.Find(Builders<EmployeeDocument>.Filter.Empty)
            .Sort(Builders<EmployeeDocument>.Sort.Ascending(x => x.FullName))
            .ToListAsync(ct);

        return documents.Select(EmployeeMapper.ToDomain).ToList().AsReadOnly();
    }

    public async Task<long> ChangeRateAsync(
        EmployeeId id,
        DateOnly fromDate,
        decimal newRate,
        CancellationToken ct)
    {
        var filter = Builders<EmployeeDocument>.Filter.Eq(x => x.Id, id.Value);

        var update = Builders<EmployeeDocument>.Update
            .AddToSet("rateHistory", new BsonDocument
            {
                { "from", fromDate.ToString("yyyy-MM-dd") },
                { "rate", newRate }
            })
            .Inc(x => x.RateRevision, 1);

        var options = new FindOneAndUpdateOptions<EmployeeDocument>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updatedDocument = await _collection.FindOneAndUpdateAsync(filter, update, options, ct);

        if (updatedDocument is null)
        {
            throw new InvalidOperationException($"Employee with id {id.Value} not found");
        }

        return updatedDocument.RateRevision;
    }

    public async Task CreateAsync(Employee employee, CancellationToken ct)
    {
        var document = EmployeeMapper.ToDocument(employee);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
    }
}

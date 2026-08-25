using MongoDB.Driver;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain.PeriodClosures;
using Timesheet.Infrastructure.MongoDb.DocumentMapping;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.Repositories;

public sealed class MongoPeriodClosureRepository : IPeriodClosureRepository
{
    private readonly IMongoCollection<PeriodClosureDocument> _collection;

    public MongoPeriodClosureRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PeriodClosureDocument>("period_closures");
    }

    public async Task<PeriodClosure?> GetAsync(int year, int month, CancellationToken ct)
    {
        var filter = Builders<PeriodClosureDocument>.Filter.And(
            Builders<PeriodClosureDocument>.Filter.Eq(x => x.Year, year),
            Builders<PeriodClosureDocument>.Filter.Eq(x => x.Month, month));

        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document is null ? null : PeriodClosureMapper.ToDomain(document);
    }

    public async Task SetClosedAsync(int year, int month, bool isClosed, CancellationToken ct)
    {
        var filter = Builders<PeriodClosureDocument>.Filter.And(
            Builders<PeriodClosureDocument>.Filter.Eq(x => x.Year, year),
            Builders<PeriodClosureDocument>.Filter.Eq(x => x.Month, month));

        var document = new PeriodClosureDocument
        {
            Year = year,
            Month = month,
            IsClosed = isClosed
        };

        var options = new ReplaceOptions { IsUpsert = true };
        await _collection.ReplaceOneAsync(filter, document, options, ct);
    }
}

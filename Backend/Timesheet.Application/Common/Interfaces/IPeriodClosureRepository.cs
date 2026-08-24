using Timesheet.Domain.PeriodClosures;

namespace Timesheet.Application.Common.Interfaces;

public interface IPeriodClosureRepository
{
    Task<PeriodClosure?> GetAsync(int year, int month, CancellationToken ct);
    Task SetClosedAsync(int year, int month, bool isClosed, CancellationToken ct);
}

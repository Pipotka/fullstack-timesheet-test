using MediatR;

namespace Timesheet.Application.PeriodClosures.Close;

public sealed record ClosePeriodCommand(int Year, int Month) : IRequest<PeriodResult>;

public sealed record PeriodResult(int Year, int Month, bool IsClosed);

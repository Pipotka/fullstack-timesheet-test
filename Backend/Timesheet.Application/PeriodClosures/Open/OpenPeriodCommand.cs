using MediatR;
using Timesheet.Application.PeriodClosures.Close;

namespace Timesheet.Application.PeriodClosures.Open;

public sealed record OpenPeriodCommand(int Year, int Month) : IRequest<PeriodResult>;

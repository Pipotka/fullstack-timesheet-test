using MediatR;
using Timesheet.Application.Common.Interfaces;

namespace Timesheet.Application.PeriodClosures.Close;

public sealed class ClosePeriodCommandHandler(
    IPeriodClosureRepository periodClosureRepository)
    : IRequestHandler<ClosePeriodCommand, PeriodResult>
{
    public async Task<PeriodResult> Handle(
        ClosePeriodCommand command,
        CancellationToken cancellationToken)
    {
        await periodClosureRepository.SetClosedAsync(
            command.Year,
            command.Month,
            true,
            cancellationToken);

        return new PeriodResult(command.Year, command.Month, true);
    }
}

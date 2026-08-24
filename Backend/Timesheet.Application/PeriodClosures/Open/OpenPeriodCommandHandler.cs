using MediatR;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.PeriodClosures.Close;

namespace Timesheet.Application.PeriodClosures.Open;

public sealed class OpenPeriodCommandHandler(
    IPeriodClosureRepository periodClosureRepository)
    : IRequestHandler<OpenPeriodCommand, PeriodResult>
{
    public async Task<PeriodResult> Handle(
        OpenPeriodCommand command,
        CancellationToken cancellationToken)
    {
        await periodClosureRepository.SetClosedAsync(
            command.Year,
            command.Month,
            false,
            cancellationToken);

        return new PeriodResult(command.Year, command.Month, false);
    }
}

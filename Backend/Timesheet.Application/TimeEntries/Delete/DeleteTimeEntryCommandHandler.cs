using MediatR;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;

namespace Timesheet.Application.TimeEntries.Delete;

public sealed class DeleteTimeEntryCommandHandler(
    ITimeEntryRepository timeEntryRepository,
    IPeriodClosureRepository periodClosureRepository)
    : IRequestHandler<DeleteTimeEntryCommand>
{
    public async Task Handle(
        DeleteTimeEntryCommand command,
        CancellationToken cancellationToken)
    {
        var existingEntry = await timeEntryRepository.GetByIdAsync(
            new TimeEntryId(command.Id),
            cancellationToken) ?? throw new BusinessException(
                ErrorCodes.TimeEntryNotFound,
                ErrorMessages.TimeEntryNotFound);

        var periodClosure = await periodClosureRepository.GetAsync(
            existingEntry.Date.Year,
            existingEntry.Date.Month,
            cancellationToken);

        if (periodClosure?.IsClosed == true)
        {
            throw new BusinessException(
                ErrorCodes.PeriodClosed,
                ErrorMessages.PeriodClosed);
        }

        await timeEntryRepository.DeleteAsync(
            new TimeEntryId(command.Id),
            cancellationToken);
    }
}

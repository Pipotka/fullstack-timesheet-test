using MediatR;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;
using Timesheet.Domain.Common;
using Timesheet.Domain.TimeEntries;

namespace Timesheet.Application.TimeEntries.Update;

public sealed class UpdateTimeEntryCommandHandler(
    ITimeEntryRepository timeEntryRepository,
    IPeriodClosureRepository periodClosureRepository)
    : IRequestHandler<UpdateTimeEntryCommand, UpdateTimeEntryResult>
{
    public async Task<UpdateTimeEntryResult> Handle(
        UpdateTimeEntryCommand command,
        CancellationToken cancellationToken)
    {
        var existingEntry = await timeEntryRepository.GetByIdAsync(
            new TimeEntryId(command.Id),
            cancellationToken);

        if (existingEntry is null)
        {
            throw new BusinessException(
                ErrorCodes.TimeEntryNotFound,
                ErrorMessages.TimeEntryNotFound);
        }

        if (existingEntry.Version != command.Version)
        {
            throw new BusinessException(
                ErrorCodes.ConcurrencyConflict,
                ErrorMessages.ConcurrencyConflict);
        }

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

        var cost = TimeEntry.CalculateCost(command.Hours, existingEntry.AppliedRate);

        var sumHours = await timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            existingEntry.EmployeeId,
            existingEntry.Date,
            existingEntry.Id,
            cancellationToken);

        if (sumHours + command.Hours > 24)
        {
            throw new BusinessException(
                ErrorCodes.DailyLimitExceeded,
                ErrorMessages.DailyLimitExceeded);
        }

        var updatedEntry = new TimeEntry
        {
            Id = existingEntry.Id,
            EmployeeId = existingEntry.EmployeeId,
            ProjectId = existingEntry.ProjectId,
            Date = existingEntry.Date,
            Hours = command.Hours,
            Comment = command.Comment,
            AppliedRate = existingEntry.AppliedRate,
            Cost = cost,
            RateRevision = existingEntry.RateRevision,
            Version = existingEntry.Version + 1
        };

        var success = await timeEntryRepository.UpdateAsync(updatedEntry, cancellationToken);

        if (!success)
        {
            throw new BusinessException(
                ErrorCodes.ConcurrencyConflict,
                ErrorMessages.ConcurrencyConflict);
        }

        return new UpdateTimeEntryResult(
            Id: updatedEntry.Id.Value,
            EmployeeId: updatedEntry.EmployeeId.Value,
            ProjectId: updatedEntry.ProjectId.Value,
            Date: updatedEntry.Date,
            Hours: updatedEntry.Hours,
            Comment: updatedEntry.Comment,
            AppliedRate: updatedEntry.AppliedRate,
            Cost: updatedEntry.Cost,
            RateRevision: updatedEntry.RateRevision,
            Version: updatedEntry.Version);
    }
}

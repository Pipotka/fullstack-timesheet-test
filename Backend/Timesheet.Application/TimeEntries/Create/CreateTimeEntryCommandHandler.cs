using MediatR;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Domain;
using Timesheet.Domain.TimeEntries;

namespace Timesheet.Application.TimeEntries.Create;

public sealed class CreateTimeEntryCommandHandler(
    ITimeEntryRepository timeEntryRepository,
    IEmployeeRepository employeeRepository,
    IProjectRepository projectRepository,
    IPeriodClosureRepository periodClosureRepository)
    : IRequestHandler<CreateTimeEntryCommand, CreateTimeEntryResult>
{
    public async Task<CreateTimeEntryResult> Handle(
        CreateTimeEntryCommand command,
        CancellationToken cancellationToken)
    {
        var periodClosure = await periodClosureRepository.GetAsync(
            command.Date.Year,
            command.Date.Month,
            cancellationToken);

        if (periodClosure?.IsClosed == true)
        {
            throw new BusinessException(
                ErrorCodes.PeriodClosed,
                ErrorMessages.PeriodClosed);
        }

        var employee = await employeeRepository.GetByIdAsync(
            new EmployeeId(command.EmployeeId),
            cancellationToken) ?? throw new BusinessException(
                ErrorCodes.EmployeeNotFound,
                ErrorMessages.EmployeeNotFound);

        var project = await projectRepository.GetByIdAsync(
            new ProjectId(command.ProjectId),
            cancellationToken) ?? throw new BusinessException(
                ErrorCodes.ProjectNotFound,
                ErrorMessages.ProjectNotFound);

        if (project.StartDate.HasValue && command.Date < project.StartDate.Value)
        {
            throw new BusinessException(
                ErrorCodes.DateOutsideProjectRange,
                ErrorMessages.DateOutsideProjectRange);
        }

        if (project.EndDate.HasValue && command.Date > project.EndDate.Value)
        {
            throw new BusinessException(
                ErrorCodes.DateOutsideProjectRange,
                ErrorMessages.DateOutsideProjectRange);
        }

        var appliedRate = employee.GetCurrentRate(command.Date) ?? throw new BusinessException(
                ErrorCodes.MissingRate,
                ErrorMessages.MissingRate);
        var cost = TimeEntry.CalculateCost(command.Hours, appliedRate);

        var sumHours = await timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId(command.EmployeeId),
            command.Date,
            null,
            cancellationToken);

        if (sumHours + command.Hours > 24)
        {
            throw new BusinessException(
                ErrorCodes.DailyLimitExceeded,
                ErrorMessages.DailyLimitExceeded);
        }

        var entry = new TimeEntry
        {
            Id = new TimeEntryId(Guid.NewGuid().ToString()),
            EmployeeId = new EmployeeId(command.EmployeeId),
            ProjectId = new ProjectId(command.ProjectId),
            Date = command.Date,
            Hours = command.Hours,
            Comment = command.Comment,
            AppliedRate = appliedRate,
            Cost = cost,
            RateRevision = employee.RateRevision,
            Version = 1
        };

        await timeEntryRepository.CreateAsync(entry, cancellationToken);

        return new CreateTimeEntryResult(
            Id: entry.Id.Value,
            EmployeeId: entry.EmployeeId.Value,
            ProjectId: entry.ProjectId.Value,
            Date: entry.Date,
            Hours: entry.Hours,
            Comment: entry.Comment,
            AppliedRate: entry.AppliedRate,
            Cost: entry.Cost,
            RateRevision: entry.RateRevision,
            Version: entry.Version);
    }
}

using MediatR;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.Common.Models;
using Timesheet.Domain;

namespace Timesheet.Application.TimeEntries.List;

public sealed class ListTimeEntriesQueryHandler(
    ITimeEntryRepository timeEntryRepository,
    IEmployeeRepository employeeRepository,
    IProjectRepository projectRepository)
    : IRequestHandler<ListTimeEntriesQuery, ListTimeEntriesResult>
{
    public async Task<ListTimeEntriesResult> Handle(
        ListTimeEntriesQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new TimeEntryFilter(
            EmployeeId: query.EmployeeId is not null ? new EmployeeId(query.EmployeeId) : null,
            ProjectId: query.ProjectId is not null ? new ProjectId(query.ProjectId) : null,
            FromDate: query.FromDate,
            ToDate: query.ToDate,
            Page: query.Page,
            PageSize: query.PageSize);

        var (items, totalCount) = await timeEntryRepository.ListAsync(filter, cancellationToken);
        var (totalHours, totalCost) = await timeEntryRepository.SumByFilterAsync(filter, cancellationToken);

        var resultItems = new List<TimeEntryItem>();

        foreach (var entry in items)
        {
            var employee = await employeeRepository.GetByIdAsync(entry.EmployeeId, cancellationToken);
            var project = await projectRepository.GetByIdAsync(entry.ProjectId, cancellationToken);

            var dailyHours = await timeEntryRepository.SumHoursByEmployeeAndDateAsync(
                entry.EmployeeId,
                entry.Date,
                null,
                cancellationToken);

            var isOvertime = dailyHours > 12;

            resultItems.Add(new TimeEntryItem(
                Id: entry.Id.Value,
                EmployeeId: entry.EmployeeId.Value,
                EmployeeName: employee?.FullName ?? string.Empty,
                ProjectId: entry.ProjectId.Value,
                ProjectCode: project?.Code ?? string.Empty,
                Date: entry.Date,
                Hours: entry.Hours,
                Comment: entry.Comment,
                AppliedRate: entry.AppliedRate,
                Cost: entry.Cost,
                IsOvertime: isOvertime));
        }

        return new ListTimeEntriesResult(
            Items: resultItems,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize,
            TotalHours: totalHours,
            TotalCost: totalCost);
    }
}

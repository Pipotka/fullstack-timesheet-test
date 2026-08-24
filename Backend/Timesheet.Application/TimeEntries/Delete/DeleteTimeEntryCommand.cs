using MediatR;

namespace Timesheet.Application.TimeEntries.Delete;

public sealed record DeleteTimeEntryCommand(string Id) : IRequest;

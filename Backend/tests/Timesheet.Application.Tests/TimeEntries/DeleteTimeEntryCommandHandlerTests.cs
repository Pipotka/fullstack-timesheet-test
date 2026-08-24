using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.TimeEntries.Delete;
using Timesheet.Domain;
using Timesheet.Domain.Common;
using Timesheet.Domain.PeriodClosures;
using Timesheet.Domain.TimeEntries;

namespace Timesheet.Application.Tests.TimeEntries;

public sealed class DeleteTimeEntryCommandHandlerTests
{
    private readonly ITimeEntryRepository _timeEntryRepository = Substitute.For<ITimeEntryRepository>();
    private readonly IPeriodClosureRepository _periodClosureRepository = Substitute.For<IPeriodClosureRepository>();

    private readonly DeleteTimeEntryCommandHandler _handler;

    public DeleteTimeEntryCommandHandlerTests()
    {
        _handler = new DeleteTimeEntryCommandHandler(
            _timeEntryRepository,
            _periodClosureRepository);
    }

    [Fact]
    public async Task Handle_EntryNotFound_ThrowsBusinessException()
    {
        var command = new DeleteTimeEntryCommand(Id: "entry-001");

        _timeEntryRepository.GetByIdAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.TimeEntryNotFound);
    }

    [Fact]
    public async Task Handle_PeriodClosed_ThrowsBusinessException()
    {
        var command = new DeleteTimeEntryCommand(Id: "entry-001");

        var existingEntry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 8, 25),
            Hours = 8.0m,
            Comment = "Test",
            AppliedRate = 1500m,
            Cost = 12000m,
            RateRevision = 1,
            Version = 1
        };

        _timeEntryRepository.GetByIdAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns(new PeriodClosure { Year = 2026, Month = 8, IsClosed = true });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.PeriodClosed);
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesEntry()
    {
        var command = new DeleteTimeEntryCommand(Id: "entry-001");

        var existingEntry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 8, 25),
            Hours = 8.0m,
            Comment = "Test",
            AppliedRate = 1500m,
            Cost = 12000m,
            RateRevision = 1,
            Version = 1
        };

        _timeEntryRepository.GetByIdAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

        _timeEntryRepository.DeleteAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _timeEntryRepository.Received(1).DeleteAsync(
            new TimeEntryId("entry-001"),
            Arg.Any<CancellationToken>());
    }
}

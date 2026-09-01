using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Errors;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.TimeEntries.Update;
using Timesheet.Domain;
using Timesheet.Domain.PeriodClosures;
using Timesheet.Domain.TimeEntries;

namespace Timesheet.Application.Tests.TimeEntries;

public sealed class UpdateTimeEntryCommandHandlerTests
{
    private readonly ITimeEntryRepository _timeEntryRepository = Substitute.For<ITimeEntryRepository>();
    private readonly IPeriodClosureRepository _periodClosureRepository = Substitute.For<IPeriodClosureRepository>();

    private readonly UpdateTimeEntryCommandHandler _handler;

    public UpdateTimeEntryCommandHandlerTests()
    {
        _handler = new UpdateTimeEntryCommandHandler(
            _timeEntryRepository,
            _periodClosureRepository);
    }

    [Fact]
    public async Task Handle_EntryNotFound_ThrowsBusinessException()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 1,
            Hours: 8.0m,
            Comment: "Test");

        _timeEntryRepository.GetByIdAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns((TimeEntry?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.TimeEntryNotFound);
    }

    [Fact]
    public async Task Handle_VersionMismatch_ThrowsBusinessException()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 2,
            Hours: 8.0m,
            Comment: "Test");

        var existingEntry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 8, 25),
            Hours = 8.0m,
            Comment = "Old comment",
            AppliedRate = 1500m,
            Cost = 12000m,
            RateRevision = 1,
            Version = 1
        };

        _timeEntryRepository.GetByIdAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.ConcurrencyConflict);
    }

    [Fact]
    public async Task Handle_PeriodClosed_ThrowsBusinessException()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 1,
            Hours: 8.0m,
            Comment: "Test");

        var existingEntry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 8, 25),
            Hours = 8.0m,
            Comment = "Old comment",
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
    public async Task Handle_DailyLimitExceeded_ThrowsBusinessException()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 1,
            Hours: 5.0m,
            Comment: "Test");

        var existingEntry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 8, 25),
            Hours = 8.0m,
            Comment = "Old comment",
            AppliedRate = 1500m,
            Cost = 12000m,
            RateRevision = 1,
            Version = 1
        };

        _timeEntryRepository.GetByIdAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            new TimeEntryId("entry-001"),
            Arg.Any<CancellationToken>())
            .Returns(20.0m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.DailyLimitExceeded);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesEntry()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 1,
            Hours: 7.5m,
            Comment: "Updated comment");

        var existingEntry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 8, 25),
            Hours = 8.0m,
            Comment = "Old comment",
            AppliedRate = 1500m,
            Cost = 12000m,
            RateRevision = 1,
            Version = 1
        };

        _timeEntryRepository.GetByIdAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            new TimeEntryId("entry-001"),
            Arg.Any<CancellationToken>())
            .Returns(0m);

        _timeEntryRepository.UpdateAsync(Arg.Any<TimeEntry>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("entry-001");
        result.Hours.Should().Be(7.5m);
        result.Comment.Should().Be("Updated comment");
        result.Version.Should().Be(2);
        result.Cost.Should().Be(11250.00m);
    }

    [Fact]
    public async Task Handle_RepoUpdateFails_ThrowsConcurrencyConflict()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 1,
            Hours: 8.0m,
            Comment: "Test");

        var existingEntry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 8, 25),
            Hours = 8.0m,
            Comment = "Old comment",
            AppliedRate = 1500m,
            Cost = 12000m,
            RateRevision = 1,
            Version = 1
        };

        _timeEntryRepository.GetByIdAsync(new TimeEntryId("entry-001"), Arg.Any<CancellationToken>())
            .Returns(existingEntry);

        _periodClosureRepository.GetAsync(2026, 8, Arg.Any<CancellationToken>())
            .Returns((PeriodClosure?)null);

        _timeEntryRepository.SumHoursByEmployeeAndDateAsync(
            new EmployeeId("emp-001"),
            new DateOnly(2026, 8, 25),
            new TimeEntryId("entry-001"),
            Arg.Any<CancellationToken>())
            .Returns(0m);

        _timeEntryRepository.UpdateAsync(Arg.Any<TimeEntry>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .Where(e => e.Code == ErrorCodes.ConcurrencyConflict);
    }
}

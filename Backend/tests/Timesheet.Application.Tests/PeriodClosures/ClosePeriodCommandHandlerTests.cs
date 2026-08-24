using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.PeriodClosures.Close;

namespace Timesheet.Application.Tests.PeriodClosures;

public sealed class ClosePeriodCommandHandlerTests
{
    private readonly IPeriodClosureRepository _periodClosureRepository = Substitute.For<IPeriodClosureRepository>();

    private readonly ClosePeriodCommandHandler _handler;

    public ClosePeriodCommandHandlerTests()
    {
        _handler = new ClosePeriodCommandHandler(_periodClosureRepository);
    }

    [Fact]
    public async Task Handle_CallsSetClosedAsync_WithTrue()
    {
        var command = new ClosePeriodCommand(Year: 2026, Month: 8);

        await _handler.Handle(command, CancellationToken.None);

        await _periodClosureRepository.Received(1).SetClosedAsync(
            2026,
            8,
            true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsCorrectResult()
    {
        var command = new ClosePeriodCommand(Year: 2026, Month: 8);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Year.Should().Be(2026);
        result.Month.Should().Be(8);
        result.IsClosed.Should().BeTrue();
    }
}

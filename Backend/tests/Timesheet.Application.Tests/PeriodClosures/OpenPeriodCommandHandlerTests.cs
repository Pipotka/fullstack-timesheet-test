using FluentAssertions;
using NSubstitute;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Application.PeriodClosures.Open;

namespace Timesheet.Application.Tests.PeriodClosures;

public sealed class OpenPeriodCommandHandlerTests
{
    private readonly IPeriodClosureRepository _periodClosureRepository = Substitute.For<IPeriodClosureRepository>();

    private readonly OpenPeriodCommandHandler _handler;

    public OpenPeriodCommandHandlerTests()
    {
        _handler = new OpenPeriodCommandHandler(_periodClosureRepository);
    }

    [Fact]
    public async Task Handle_CallsSetClosedAsync_WithFalse()
    {
        var command = new OpenPeriodCommand(Year: 2026, Month: 8);

        await _handler.Handle(command, CancellationToken.None);

        await _periodClosureRepository.Received(1).SetClosedAsync(
            2026,
            8,
            false,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsCorrectResult()
    {
        var command = new OpenPeriodCommand(Year: 2026, Month: 8);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Year.Should().Be(2026);
        result.Month.Should().Be(8);
        result.IsClosed.Should().BeFalse();
    }
}

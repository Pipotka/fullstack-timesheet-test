using FluentAssertions;
using Timesheet.Domain.PeriodClosures;

namespace Timesheet.Domain.Tests.PeriodClosures;

public sealed class PeriodClosureTests
{
    [Fact]
    public void PeriodClosure_CanBeCreated_Closed()
    {
        var period = new PeriodClosure
        {
            Year = 2026,
            Month = 8,
            IsClosed = true
        };

        period.Year.Should().Be(2026);
        period.Month.Should().Be(8);
        period.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void PeriodClosure_CanBeCreated_Open()
    {
        var period = new PeriodClosure
        {
            Year = 2026,
            Month = 8,
            IsClosed = false
        };

        period.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void PeriodClosure_MonthRange_1To12()
    {
        var jan = new PeriodClosure { Year = 2026, Month = 1, IsClosed = false };
        var dec = new PeriodClosure { Year = 2026, Month = 12, IsClosed = false };

        jan.Month.Should().Be(1);
        dec.Month.Should().Be(12);
    }
}

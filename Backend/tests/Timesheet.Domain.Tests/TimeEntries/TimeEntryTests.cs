using FluentAssertions;
using Timesheet.Domain.TimeEntries;

namespace Timesheet.Domain.Tests.TimeEntries;

public sealed class TimeEntryTests
{
    [Fact]
    public void CalculateCost_RoundsCorrectly()
    {
        var cost = TimeEntry.CalculateCost(8.0m, 1500.00m);

        cost.Should().Be(12000.00m);
    }

    [Fact]
    public void CalculateCost_RoundsHalfUp()
    {
        var cost = TimeEntry.CalculateCost(1.005m, 1000m);

        cost.Should().Be(1005.00m);
    }

    [Fact]
    public void CalculateCost_SmallHours()
    {
        var cost = TimeEntry.CalculateCost(0.5m, 100.00m);

        cost.Should().Be(50.00m);
    }

    [Fact]
    public void CalculateCost_ZeroHours_ReturnsZero()
    {
        var cost = TimeEntry.CalculateCost(0m, 1500m);

        cost.Should().Be(0m);
    }

    [Fact]
    public void CalculateCost_LargeValues_DoesNotOverflow()
    {
        var cost = TimeEntry.CalculateCost(1000m, 999999.99m);

        cost.Should().Be(999999990.00m);
    }
}

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

    [Fact]
    public void ValidateInvariants_ThrowsBusinessException_WhenHoursZero()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 6, 15),
            Hours = 0m,
            AppliedRate = 1500m,
            Cost = 0m,
            Version = 1
        };

        var act = () => entry.ValidateInvariants();

        act.Should().Throw<BusinessException>()
            .Where(e => e.Code == "INVALID_HOURS");
    }

    [Fact]
    public void ValidateInvariants_ThrowsBusinessException_WhenHoursNegative()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 6, 15),
            Hours = -1m,
            AppliedRate = 1500m,
            Cost = 0m,
            Version = 1
        };

        var act = () => entry.ValidateInvariants();

        act.Should().Throw<BusinessException>()
            .Where(e => e.Code == "INVALID_HOURS");
    }

    [Fact]
    public void ValidateInvariants_ThrowsBusinessException_WhenHoursExceeds24()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 6, 15),
            Hours = 25m,
            AppliedRate = 1500m,
            Cost = 37500m,
            Version = 1
        };

        var act = () => entry.ValidateInvariants();

        act.Should().Throw<BusinessException>()
            .Where(e => e.Code == "INVALID_HOURS");
    }

    [Fact]
    public void ValidateInvariants_ThrowsBusinessException_WhenHoursNotMultipleOfHalf()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 6, 15),
            Hours = 3.7m,
            AppliedRate = 1500m,
            Cost = 5550m,
            Version = 1
        };

        var act = () => entry.ValidateInvariants();

        act.Should().Throw<BusinessException>()
            .Where(e => e.Code == "INVALID_HOURS");
    }

    [Fact]
    public void ValidateInvariants_DoesNotThrow_WhenValidHours()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 6, 15),
            Hours = 8.5m,
            AppliedRate = 1500m,
            Cost = 12750m,
            Version = 1
        };

        var act = () => entry.ValidateInvariants();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateInvariants_DoesNotThrow_WhenHoursExactly24()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 6, 15),
            Hours = 24m,
            AppliedRate = 1500m,
            Cost = 36000m,
            Version = 1
        };

        var act = () => entry.ValidateInvariants();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateInvariants_ThrowsBusinessException_WhenAppliedRateNegative()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 6, 15),
            Hours = 8m,
            AppliedRate = -100m,
            Cost = -800m,
            Version = 1
        };

        var act = () => entry.ValidateInvariants();

        act.Should().Throw<BusinessException>()
            .Where(e => e.Code == "INVALID_APPLIED_RATE");
    }

    [Fact]
    public void ValidateInvariants_DoesNotThrow_WhenAppliedRateZero()
    {
        var entry = new TimeEntry
        {
            Id = new TimeEntryId("entry-001"),
            EmployeeId = new EmployeeId("emp-001"),
            ProjectId = new ProjectId("proj-001"),
            Date = new DateOnly(2026, 6, 15),
            Hours = 8m,
            AppliedRate = 0m,
            Cost = 0m,
            Version = 1
        };

        var act = () => entry.ValidateInvariants();

        act.Should().NotThrow();
    }
}

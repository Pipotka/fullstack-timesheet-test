using FluentAssertions;
using Timesheet.Domain.Employees;

namespace Timesheet.Domain.Tests.Employees;

public sealed class EmployeeTests
{
    private static Employee CreateEmployee(
        IReadOnlyList<RateHistoryEntry> history,
        long revision = 1)
        => new()
        {
            Id = new EmployeeId("emp-001"),
            FullName = "Иванов Иван Иванович",
            RateHistory = history,
            RateRevision = revision
        };

    [Fact]
    public void GetCurrentRate_ReturnsRateForDateInRange()
    {
        var history = new List<RateHistoryEntry>
        {
            new() { From = new DateOnly(2026, 1, 1), Rate = 1000m },
            new() { From = new DateOnly(2026, 4, 1), Rate = 1500m }
        };
        var employee = CreateEmployee(history);

        var rate = employee.GetCurrentRate(new DateOnly(2026, 6, 15));

        rate.Should().Be(1500m);
    }

    [Fact]
    public void GetCurrentRate_ReturnsFirstRate_WhenDateBeforeAllEntries()
    {
        var history = new List<RateHistoryEntry>
        {
            new() { From = new DateOnly(2026, 4, 1), Rate = 1500m }
        };
        var employee = CreateEmployee(history);

        var rate = employee.GetCurrentRate(new DateOnly(2026, 1, 1));

        rate.Should().Be(1500m);
    }

    [Fact]
    public void GetCurrentRate_ReturnsExactMatchRate()
    {
        var history = new List<RateHistoryEntry>
        {
            new() { From = new DateOnly(2026, 1, 1), Rate = 1000m },
            new() { From = new DateOnly(2026, 4, 1), Rate = 1500m }
        };
        var employee = CreateEmployee(history);

        var rate = employee.GetCurrentRate(new DateOnly(2026, 4, 1));

        rate.Should().Be(1500m);
    }

    [Fact]
    public void GetCurrentRate_ReturnsMiddleRate_WhenBetweenEntries()
    {
        var history = new List<RateHistoryEntry>
        {
            new() { From = new DateOnly(2026, 1, 1), Rate = 1000m },
            new() { From = new DateOnly(2026, 4, 1), Rate = 1500m },
            new() { From = new DateOnly(2026, 7, 1), Rate = 2000m }
        };
        var employee = CreateEmployee(history);

        var rate = employee.GetCurrentRate(new DateOnly(2026, 5, 15));

        rate.Should().Be(1500m);
    }

    [Fact]
    public void GetCurrentRate_ThrowsBusinessException_WhenHistoryEmpty()
    {
        var employee = CreateEmployee(new List<RateHistoryEntry>());

        var act = () => employee.GetCurrentRate(new DateOnly(2026, 6, 15));

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void ValidateInvariants_ThrowsBusinessException_WhenHistoryEmpty()
    {
        var employee = CreateEmployee(new List<RateHistoryEntry>());

        var act = () => employee.ValidateInvariants();

        act.Should().Throw<BusinessException>()
            .Where(e => e.Code == "MISSING_RATE");
    }

    [Fact]
    public void ValidateInvariants_ThrowsBusinessException_WhenDuplicateRateDates()
    {
        var history = new List<RateHistoryEntry>
        {
            new() { From = new DateOnly(2026, 1, 1), Rate = 1000m },
            new() { From = new DateOnly(2026, 1, 1), Rate = 1500m }
        };
        var employee = CreateEmployee(history);

        var act = () => employee.ValidateInvariants();

        act.Should().Throw<BusinessException>()
            .Where(e => e.Code == "DUPLICATE_RATE_DATE");
    }

    [Fact]
    public void ValidateInvariants_ThrowsBusinessException_WhenHistoryNotSorted()
    {
        var history = new List<RateHistoryEntry>
        {
            new() { From = new DateOnly(2026, 4, 1), Rate = 1500m },
            new() { From = new DateOnly(2026, 1, 1), Rate = 1000m }
        };
        var employee = CreateEmployee(history);

        var act = () => employee.ValidateInvariants();

        act.Should().Throw<BusinessException>()
            .Where(e => e.Code == "INVALID_RATE_HISTORY");
    }

    [Fact]
    public void ValidateInvariants_DoesNotThrow_WhenValidHistory()
    {
        var history = new List<RateHistoryEntry>
        {
            new() { From = new DateOnly(2026, 1, 1), Rate = 1000m },
            new() { From = new DateOnly(2026, 4, 1), Rate = 1500m }
        };
        var employee = CreateEmployee(history);

        var act = () => employee.ValidateInvariants();

        act.Should().NotThrow();
    }
}

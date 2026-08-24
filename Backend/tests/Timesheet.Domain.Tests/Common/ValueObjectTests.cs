using FluentAssertions;

namespace Timesheet.Domain.Tests.Common;

public sealed class ValueObjectTests
{
    [Fact]
    public void EmployeeId_Equality_SameValue()
    {
        var a = new EmployeeId("emp-001");
        var b = new EmployeeId("emp-001");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void EmployeeId_Inequality_DifferentValue()
    {
        var a = new EmployeeId("emp-001");
        var b = new EmployeeId("emp-002");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void ProjectId_Equality_SameValue()
    {
        var a = new ProjectId("proj-001");
        var b = new ProjectId("proj-001");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void ProjectId_Inequality_DifferentValue()
    {
        var a = new ProjectId("proj-001");
        var b = new ProjectId("proj-002");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void TimeEntryId_Equality_SameValue()
    {
        var a = new TimeEntryId("entry-001");
        var b = new TimeEntryId("entry-001");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void TimeEntryId_Inequality_DifferentValue()
    {
        var a = new TimeEntryId("entry-001");
        var b = new TimeEntryId("entry-002");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void DateRange_Valid_FromLessThanTo()
    {
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 31);

        var act = () => new DateRange(from, to);

        act.Should().NotThrow();
    }

    [Fact]
    public void DateRange_Valid_FromEqualsTo()
    {
        var date = new DateOnly(2026, 1, 1);

        var act = () => new DateRange(date, date);

        act.Should().NotThrow();
    }

    [Fact]
    public void DateRange_Invalid_FromGreaterThanTo()
    {
        var from = new DateOnly(2026, 2, 1);
        var to = new DateOnly(2026, 1, 1);

        var act = () => new DateRange(from, to);

        act.Should().Throw<ArgumentException>();
    }
}

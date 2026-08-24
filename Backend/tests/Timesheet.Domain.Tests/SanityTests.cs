using FluentAssertions;

namespace Timesheet.Domain.Tests;

public class SanityTests
{
    [Fact]
    public void Infrastructure_Is_Ready()
    {
        true.Should().BeTrue();
    }
}

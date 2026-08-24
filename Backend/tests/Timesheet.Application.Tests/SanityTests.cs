using FluentAssertions;

namespace Timesheet.Application.Tests;

public class SanityTests
{
    [Fact]
    public void Infrastructure_Is_Ready()
    {
        true.Should().BeTrue();
    }
}

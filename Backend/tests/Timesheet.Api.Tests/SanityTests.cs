using FluentAssertions;

namespace Timesheet.Api.Tests;

public class SanityTests
{
    [Fact]
    public void Infrastructure_Is_Ready()
    {
        true.Should().BeTrue();
    }
}

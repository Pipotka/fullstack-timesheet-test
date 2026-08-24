using FluentAssertions;

namespace Timesheet.Infrastructure.Tests;

public class SanityTests
{
    [Fact]
    public void Infrastructure_Is_Ready()
    {
        true.Should().BeTrue();
    }
}

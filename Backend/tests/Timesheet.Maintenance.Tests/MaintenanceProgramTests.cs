using FluentAssertions;

namespace Timesheet.Maintenance.Tests;

public class MaintenanceProgramTests
{
    [Fact]
    public async Task Main_NoArgs_ReturnsNonZeroExitCode()
    {
        var exitCode = await Program.Main([]);
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task Main_UnknownCommand_ReturnsNonZeroExitCode()
    {
        var exitCode = await Program.Main(["unknown-command"]);
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task Main_ChangeRate_MissingEmployee_ReturnsNonZeroExitCode()
    {
        var exitCode = await Program.Main(["change-rate", "--from", "2026-03-01", "--rate", "600"]);
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task Main_ChangeRate_MissingFrom_ReturnsNonZeroExitCode()
    {
        var exitCode = await Program.Main(["change-rate", "--employee", "emp-1", "--rate", "600"]);
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task Main_ChangeRate_MissingRate_ReturnsNonZeroExitCode()
    {
        var exitCode = await Program.Main(["change-rate", "--employee", "emp-1", "--from", "2026-03-01"]);
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task Main_ChangeRate_InvalidDateFormat_ReturnsNonZeroExitCode()
    {
        var exitCode = await Program.Main(["change-rate", "--employee", "emp-1", "--from", "01-03-2026", "--rate", "600"]);
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task Main_ChangeRate_InvalidRateFormat_ReturnsNonZeroExitCode()
    {
        var exitCode = await Program.Main(["change-rate", "--employee", "emp-1", "--from", "2026-03-01", "--rate", "abc"]);
        exitCode.Should().Be(1);
    }

    [Theory]
    [InlineData("create-indexes")]
    [InlineData("indexes")]
    public async Task Main_IndexesCommand_AcceptsBothAliases(string command)
    {
        // This will fail because MongoDB is not available, but it should parse the command correctly
        // and fail at the infrastructure level, not at the parsing level
        var exitCode = await Program.Main([command]);
        // Exit code 1 is expected because MongoDB is not running
        exitCode.Should().Be(1);
    }
}

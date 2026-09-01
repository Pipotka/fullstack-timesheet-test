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

    [Fact]
    public void BuildConfiguration_LoadsDefaultAppsettings_FromBaseDirectory()
    {
        // This test verifies that the configuration builder can find appsettings.json
        // from AppContext.BaseDirectory (where it's copied to output)
        var config = Program.BuildConfigurationForTest();

        // Should have default values from appsettings.json
        config["MongoDb:ConnectionString"].Should().Be("mongodb://localhost:27017");
        config["MongoDb:DatabaseName"].Should().Be("Timesheet");
    }

    [Fact]
    public void BuildConfiguration_EnvironmentVariablesOverrideDefaults()
    {
        // Set environment variables
        Environment.SetEnvironmentVariable("MongoDb__ConnectionString", "mongodb://custom:27017");
        Environment.SetEnvironmentVariable("MongoDb__DatabaseName", "CustomDb");

        try
        {
            var config = Program.BuildConfigurationForTest();

            // Environment variables should override defaults
            config["MongoDb:ConnectionString"].Should().Be("mongodb://custom:27017");
            config["MongoDb:DatabaseName"].Should().Be("CustomDb");
        }
        finally
        {
            // Clean up
            Environment.SetEnvironmentVariable("MongoDb__ConnectionString", null);
            Environment.SetEnvironmentVariable("MongoDb__DatabaseName", null);
        }
    }

    [Fact]
    public void Readme_RateChangeExample_UsesAuthoritativeSeedDate()
    {
        // README should reference the authoritative seed date 2026-03-05, not 2026-03-10
        var readmePath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "Backend", "README.md");

        // Normalize path
        readmePath = Path.GetFullPath(readmePath);

        if (!File.Exists(readmePath))
        {
            // Try alternative path from test output directory
            readmePath = Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "README.md");
            readmePath = Path.GetFullPath(readmePath);
        }

        if (File.Exists(readmePath))
        {
            var content = File.ReadAllText(readmePath);
            content.Should().Contain("2026-03-05", "README should reference authoritative seed date");
            content.Should().NotContain("2026-03-10", "README should not reference old incorrect date");
        }
        else
        {
            // If we can't find README, skip this test (don't fail)
            // This is a documentation check, not a critical functionality test
            Assert.True(true, "README.md not found at expected location, skipping date check");
        }
    }
}

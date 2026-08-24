using FluentAssertions;
using Timesheet.Application.TimeEntries.Create;
using Timesheet.Application.TimeEntries.Update;

namespace Timesheet.Application.Tests.TimeEntries;

public sealed class TimeEntryValidatorTests
{
    private readonly CreateTimeEntryCommandValidator _createValidator = new();
    private readonly UpdateTimeEntryCommandValidator _updateValidator = new();

    #region Create Validator

    [Fact]
    public void Create_WithEmptyEmployeeId_Fails()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTimeEntryCommand.EmployeeId));
    }

    [Fact]
    public void Create_WithEmptyProjectId_Fails()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTimeEntryCommand.ProjectId));
    }

    [Fact]
    public void Create_WithZeroHours_Fails()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 0m,
            Comment: "Test");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTimeEntryCommand.Hours));
    }

    [Fact]
    public void Create_WithNegativeHours_Fails()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: -1m,
            Comment: "Test");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTimeEntryCommand.Hours));
    }

    [Fact]
    public void Create_WithHoursExceeding24_Fails()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 24.5m,
            Comment: "Test");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTimeEntryCommand.Hours));
    }

    [Fact]
    public void Create_WithHoursNotMultipleOf05_Fails()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 1.3m,
            Comment: "Test");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTimeEntryCommand.Hours));
    }

    [Fact]
    public void Create_WithCommentExceedingMaxLength_Fails()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: new string('a', 1001));

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTimeEntryCommand.Comment));
    }

    [Fact]
    public void Create_WithValidData_Passes()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 8.0m,
            Comment: "Test comment");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_WithHoursExactly24_Passes()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 24.0m,
            Comment: "Test");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_WithHoursMultipleOf05_Passes()
    {
        var command = new CreateTimeEntryCommand(
            EmployeeId: "emp-001",
            ProjectId: "proj-001",
            Date: new DateOnly(2026, 8, 25),
            Hours: 7.5m,
            Comment: "Test");

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Update Validator

    [Fact]
    public void Update_WithEmptyId_Fails()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "",
            Version: 1,
            Hours: 8.0m,
            Comment: "Test");

        var result = _updateValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTimeEntryCommand.Id));
    }

    [Fact]
    public void Update_WithVersionLessThan1_Fails()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 0,
            Hours: 8.0m,
            Comment: "Test");

        var result = _updateValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTimeEntryCommand.Version));
    }

    [Fact]
    public void Update_WithZeroHours_Fails()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 1,
            Hours: 0m,
            Comment: "Test");

        var result = _updateValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTimeEntryCommand.Hours));
    }

    [Fact]
    public void Update_WithHoursNotMultipleOf05_Fails()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 1,
            Hours: 1.3m,
            Comment: "Test");

        var result = _updateValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTimeEntryCommand.Hours));
    }

    [Fact]
    public void Update_WithValidData_Passes()
    {
        var command = new UpdateTimeEntryCommand(
            Id: "entry-001",
            Version: 1,
            Hours: 8.0m,
            Comment: "Test comment");

        var result = _updateValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}

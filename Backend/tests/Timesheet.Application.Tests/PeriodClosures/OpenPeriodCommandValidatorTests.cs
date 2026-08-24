using FluentAssertions;
using FluentValidation;
using Timesheet.Application.PeriodClosures.Open;

namespace Timesheet.Application.Tests.PeriodClosures;

public sealed class OpenPeriodCommandValidatorTests
{
    private readonly IValidator<OpenPeriodCommand> _validator = new OpenPeriodCommandValidator();

    [Fact]
    public void Validate_InvalidMonth_HasError()
    {
        var command = new OpenPeriodCommand(Year: 2026, Month: 0);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Month");
    }

    [Fact]
    public void Validate_MonthAbove12_HasError()
    {
        var command = new OpenPeriodCommand(Year: 2026, Month: 13);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Month");
    }

    [Fact]
    public void Validate_InvalidYear_HasError()
    {
        var command = new OpenPeriodCommand(Year: 0, Month: 8);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Year");
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new OpenPeriodCommand(Year: 2026, Month: 8);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}

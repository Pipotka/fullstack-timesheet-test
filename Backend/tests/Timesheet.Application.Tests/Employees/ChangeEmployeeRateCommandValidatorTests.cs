using FluentAssertions;
using FluentValidation;
using Timesheet.Application.Employees.ChangeRate;

namespace Timesheet.Application.Tests.Employees;

public sealed class ChangeEmployeeRateCommandValidatorTests
{
    private readonly IValidator<ChangeEmployeeRateCommand> _validator = new ChangeEmployeeRateCommandValidator();

    [Fact]
    public void Validate_EmptyEmployeeId_HasError()
    {
        var command = new ChangeEmployeeRateCommand(
            EmployeeId: string.Empty,
            FromDate: new DateOnly(2026, 4, 1),
            NewRate: 1800m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmployeeId");
    }

    [Fact]
    public void Validate_NegativeRate_HasError()
    {
        var command = new ChangeEmployeeRateCommand(
            EmployeeId: "emp-001",
            FromDate: new DateOnly(2026, 4, 1),
            NewRate: -1m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewRate");
    }

    [Fact]
    public void Validate_ZeroRate_Passes()
    {
        var command = new ChangeEmployeeRateCommand(
            EmployeeId: "emp-001",
            FromDate: new DateOnly(2026, 4, 1),
            NewRate: 0m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new ChangeEmployeeRateCommand(
            EmployeeId: "emp-001",
            FromDate: new DateOnly(2026, 4, 1),
            NewRate: 1800m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}

using FluentValidation;

namespace Timesheet.Application.Employees.ChangeRate;

public sealed class ChangeEmployeeRateCommandValidator : AbstractValidator<ChangeEmployeeRateCommand>
{
    public ChangeEmployeeRateCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage("EmployeeId не может быть пустым");

        RuleFor(x => x.NewRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Ставка не может быть отрицательной");
    }
}

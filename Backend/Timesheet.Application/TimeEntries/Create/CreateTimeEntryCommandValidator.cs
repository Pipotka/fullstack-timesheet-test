using FluentValidation;

namespace Timesheet.Application.TimeEntries.Create;

public sealed class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage("EmployeeId не может быть пустым");

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId не может быть пустым");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Дата не может быть пустой");

        RuleFor(x => x.Hours)
            .GreaterThan(0)
            .WithMessage("Количество часов должно быть больше 0")
            .LessThanOrEqualTo(24)
            .WithMessage("Количество часов не может превышать 24")
            .Must(BeMultipleOfHalf)
            .WithMessage("Количество часов должно быть кратно 0.5");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("Комментарий не может превышать 1000 символов");
    }

    private static bool BeMultipleOfHalf(decimal hours)
    {
        var remainder = hours % 0.5m;
        return remainder == 0;
    }
}

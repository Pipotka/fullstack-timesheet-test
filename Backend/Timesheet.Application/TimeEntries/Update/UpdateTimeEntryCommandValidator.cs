using FluentValidation;

namespace Timesheet.Application.TimeEntries.Update;

public sealed class UpdateTimeEntryCommandValidator : AbstractValidator<UpdateTimeEntryCommand>
{
    public UpdateTimeEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id не может быть пустым");

        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Версия должна быть больше или равна 1");

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

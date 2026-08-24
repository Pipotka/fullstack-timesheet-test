using FluentValidation;

namespace Timesheet.Application.TimeEntries.List;

public sealed class ListTimeEntriesQueryValidator : AbstractValidator<ListTimeEntriesQuery>
{
    public ListTimeEntriesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы должен быть больше или равен 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
            .WithMessage("Размер страницы должен быть от 1 до 200");
    }
}

using FluentValidation;

namespace Timesheet.Application.Reports.ProjectReport;

public sealed class ProjectReportQueryValidator : AbstractValidator<ProjectReportQuery>
{
    public ProjectReportQueryValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThan(0)
            .WithMessage("Год должен быть больше 0");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Месяц должен быть от 1 до 12");
    }
}

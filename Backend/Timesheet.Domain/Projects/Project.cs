namespace Timesheet.Domain.Projects;

public sealed class Project
{
    public ProjectId Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Budget { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public void ValidateInvariants()
    {
        if (Budget < 0)
        {
            throw new BusinessException(
                "INVALID_BUDGET",
                "Бюджет проекта не может быть отрицательным");
        }

        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
        {
            throw new BusinessException(
                "INVALID_DATE_RANGE",
                "Дата начала проекта не может быть позже даты окончания");
        }
    }
}

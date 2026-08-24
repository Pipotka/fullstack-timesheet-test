namespace Timesheet.Application.Common.Errors;

public static class ErrorMessages
{
    public const string DailyLimitExceeded =
        "Суммарное количество часов за день не может превышать 24";

    public const string EmployeeNotFound =
        "Сотрудник не найден";

    public const string ProjectNotFound =
        "Проект не найден";

    public const string TimeEntryNotFound =
        "Запись табеля не найдена";

    public const string PeriodClosed =
        "Период закрыт для изменений";

    public const string ConcurrencyConflict =
        "Конфликт версий: запись была изменена другим пользователем";

    public const string MissingRate =
        "Не найдена действующая ставка на указанную дату";

    public const string DuplicateRateDate =
        "Ставка на указанную дату уже существует";

    public const string InvalidDateRange =
        "Начало периода не может быть позже конца";
}

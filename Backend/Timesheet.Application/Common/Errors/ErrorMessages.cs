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

    public const string InvalidRateHistory =
        "История ставок должна быть отсортирована по возрастанию даты";

    public const string InvalidBudget =
        "Бюджет проекта не может быть отрицательным";

    public const string InvalidDateRange =
        "Дата начала не может быть позже даты окончания";

    public const string InvalidHours =
        "Количество часов должно быть больше 0, не превышать 24 и быть кратно 0.5";

    public const string InvalidAppliedRate =
        "Применённая ставка не может быть отрицательной";

    public const string InvalidPeriod =
        "Некорректный период: год должен быть больше 0, месяц от 1 до 12";
}

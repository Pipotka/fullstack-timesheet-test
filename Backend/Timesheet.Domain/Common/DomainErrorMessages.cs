namespace Timesheet.Domain.Common;

public static class DomainErrorMessages
{
    public const string MissingRate = "История ставок сотрудника пуста";
    public const string DuplicateRateDate = "Дублирующаяся дата ставки";
    public const string InvalidRateHistory = "История ставок должна быть отсортирована по возрастанию даты";
    public const string InvalidBudget = "Бюджет проекта не может быть отрицательным";
    public const string InvalidDateRange = "Дата начала не может быть позже даты окончания";
    public const string InvalidHours = "Количество часов должно быть больше 0, не превышать 24 и быть кратно 0.5";
    public const string InvalidAppliedRate = "Применённая ставка не может быть отрицательной";
    public const string InvalidPeriod = "Некорректный период: год должен быть больше 0, месяц от 1 до 12";
}

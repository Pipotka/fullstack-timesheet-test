# План: реализация backend Timesheet API

> Дата: 2026-08-25  
> Основа: `docs/superpowers/specs/2026-08-25-timesheet-backend-design.md`  
> Goal: реализовать REST API системы учёта рабочего времени — CRUD записей табеля, управление ставками сотрудников с автоматическим пересчётом стоимости, отчёты по проектам, закрытие периодов — на .NET 8 с Clean Architecture + Vertical Slices, CQRS через MediatR, FluentValidation, MongoDB.Driver без ORM.  
> Architecture: Clean Architecture (Domain → Application → Infrastructure → Api) + Vertical Slice Architecture внутри Application (группировка по фичам). CQRS через MediatR. FluentValidation pipeline behavior. MongoDB.Driver напрямую (без ORM). Stored cost (AppliedRate, Cost в TimeEntry). Monotonic RateRevision. DB-side recalculation через aggregation update pipeline. Optimistic concurrency для Update по Id+Version. Maintenance tool — отдельное консольное приложение.  
> Tech Stack: .NET 8, C# 12, ASP.NET Core Web API, MediatR 12.x, FluentValidation 11.x, MongoDB.Driver 3.x, xUnit, FluentAssertions 7.x, NSubstitute 5.x, Microsoft.AspNetCore.Mvc.Testing, coverlet, Swashbuckle, Docker Compose (standalone MongoDB 7.0). Central Package Management.  
> Методология: [subagent-driven-development](../skills/subagent-driven-development.md)

---

## Карта файлов

Все пути указаны относительно корня репозитория. Существующие файлы, которые будут изменены, помечены `(изменить)`. Новые файлы помечены `(создать)`.

### Domain

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Domain/Common/EmployeeId.cs` | создать |
| `Backend/Timesheet.Domain/Common/ProjectId.cs` | создать |
| `Backend/Timesheet.Domain/Common/TimeEntryId.cs` | создать |
| `Backend/Timesheet.Domain/Common/DateRange.cs` | создать |
| `Backend/Timesheet.Domain/Common/BusinessException.cs` | создать |
| `Backend/Timesheet.Domain/Employees/Employee.cs` | создать |
| `Backend/Timesheet.Domain/Employees/RateHistoryEntry.cs` | создать |
| `Backend/Timesheet.Domain/Projects/Project.cs` | создать |
| `Backend/Timesheet.Domain/TimeEntries/TimeEntry.cs` | создать |
| `Backend/Timesheet.Domain/PeriodClosures/PeriodClosure.cs` | создать |

### Application — Common

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Application/Common/Errors/ErrorCodes.cs` | создать |
| `Backend/Timesheet.Application/Common/Errors/ErrorMessages.cs` | создать |
| `Backend/Timesheet.Application/Common/Interfaces/ITimeEntryRepository.cs` | создать |
| `Backend/Timesheet.Application/Common/Interfaces/IEmployeeRepository.cs` | создать |
| `Backend/Timesheet.Application/Common/Interfaces/IProjectRepository.cs` | создать |
| `Backend/Timesheet.Application/Common/Interfaces/IPeriodClosureRepository.cs` | создать |
| `Backend/Timesheet.Application/Common/Models/TimeEntryFilter.cs` | создать |
| `Backend/Timesheet.Application/Common/Validation/SampleRequest.cs` | удалить |

### Application — TimeEntries

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Application/TimeEntries/Create/CreateTimeEntryCommand.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/Create/CreateTimeEntryCommandHandler.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/Create/CreateTimeEntryCommandValidator.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/Update/UpdateTimeEntryCommand.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/Update/UpdateTimeEntryCommandHandler.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/Update/UpdateTimeEntryCommandValidator.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/Delete/DeleteTimeEntryCommand.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/Delete/DeleteTimeEntryCommandHandler.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/List/ListTimeEntriesQuery.cs` | создать |
| `Backend/Timesheet.Application/TimeEntries/List/ListTimeEntriesQueryHandler.cs` | создать |

### Application — Employees

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Application/Employees/ChangeRate/ChangeEmployeeRateCommand.cs` | создать |
| `Backend/Timesheet.Application/Employees/ChangeRate/ChangeEmployeeRateCommandHandler.cs` | создать |
| `Backend/Timesheet.Application/Employees/ChangeRate/ChangeEmployeeRateCommandValidator.cs` | создать |
| `Backend/Timesheet.Application/Employees/RecalculateCosts/RecalculateCostsCommand.cs` | создать |
| `Backend/Timesheet.Application/Employees/RecalculateCosts/RecalculateCostsCommandHandler.cs` | создать |
| `Backend/Timesheet.Application/Employees/List/ListEmployeesQuery.cs` | создать |
| `Backend/Timesheet.Application/Employees/List/ListEmployeesQueryHandler.cs` | создать |

### Application — Projects

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Application/Projects/List/ListProjectsQuery.cs` | создать |
| `Backend/Timesheet.Application/Projects/List/ListProjectsQueryHandler.cs` | создать |

### Application — Reports

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Application/Reports/ProjectReport/ProjectReportQuery.cs` | создать |
| `Backend/Timesheet.Application/Reports/ProjectReport/ProjectReportQueryHandler.cs` | создать |

### Application — PeriodClosures

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Application/PeriodClosures/Close/ClosePeriodCommand.cs` | создать |
| `Backend/Timesheet.Application/PeriodClosures/Close/ClosePeriodCommandHandler.cs` | создать |
| `Backend/Timesheet.Application/PeriodClosures/Open/OpenPeriodCommand.cs` | создать |
| `Backend/Timesheet.Application/PeriodClosures/Open/OpenPeriodCommandHandler.cs` | создать |

### Infrastructure

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Infrastructure/MongoDb/Mappings/BsonClassMapConfigurator.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Mappings/DateOnlySerializer.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Documents/EmployeeDocument.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Documents/TimeEntryDocument.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Documents/ProjectDocument.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Documents/PeriodClosureDocument.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/EmployeeMapper.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/TimeEntryMapper.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/ProjectMapper.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/PeriodClosureMapper.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoTimeEntryRepository.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoEmployeeRepository.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoProjectRepository.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoPeriodClosureRepository.cs` | создать |
| `Backend/Timesheet.Infrastructure/MongoDb/Indexes/IndexCreator.cs` | создать |
| `Backend/Timesheet.Infrastructure/Maintenance/SeedData.cs` | создать |
| `Backend/Timesheet.Infrastructure/Maintenance/RateChangeService.cs` | создать |
| `Backend/Timesheet.Infrastructure/DependencyInjection.cs` | изменить |

### Api

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Api/Contracts/ErrorResponse.cs` | создать |
| `Backend/Timesheet.Api/Contracts/TimeEntries/CreateTimeEntryRequest.cs` | создать |
| `Backend/Timesheet.Api/Contracts/TimeEntries/UpdateTimeEntryRequest.cs` | создать |
| `Backend/Timesheet.Api/Contracts/TimeEntries/TimeEntryResponse.cs` | создать |
| `Backend/Timesheet.Api/Contracts/TimeEntries/ListTimeEntriesResponse.cs` | создать |
| `Backend/Timesheet.Api/Contracts/Employees/EmployeeResponse.cs` | создать |
| `Backend/Timesheet.Api/Contracts/Projects/ProjectResponse.cs` | создать |
| `Backend/Timesheet.Api/Contracts/Reports/ProjectReportResponse.cs` | создать |
| `Backend/Timesheet.Api/Contracts/Periods/PeriodRequest.cs` | создать |
| `Backend/Timesheet.Api/Contracts/Periods/PeriodResponse.cs` | создать |
| `Backend/Timesheet.Api/Middleware/ExceptionHandlingMiddleware.cs` | создать |
| `Backend/Timesheet.Api/Controllers/TimeEntriesController.cs` | создать |
| `Backend/Timesheet.Api/Controllers/EmployeesController.cs` | создать |
| `Backend/Timesheet.Api/Controllers/ProjectsController.cs` | создать |
| `Backend/Timesheet.Api/Controllers/ReportsController.cs` | создать |
| `Backend/Timesheet.Api/Controllers/PeriodsController.cs` | создать |
| `Backend/Timesheet.Api/Program.cs` | изменить |

### Maintenance

| Файл | Действие |
|---|---|
| `Backend/Timesheet.Maintenance/Timesheet.Maintenance.csproj` | создать |
| `Backend/Timesheet.Maintenance/Program.cs` | создать |

### Tests — Domain

| Файл | Действие |
|---|---|
| `Backend/tests/Timesheet.Domain.Tests/Common/ValueObjectTests.cs` | создать |
| `Backend/tests/Timesheet.Domain.Tests/Common/BusinessExceptionTests.cs` | создать |
| `Backend/tests/Timesheet.Domain.Tests/Employees/EmployeeTests.cs` | создать |
| `Backend/tests/Timesheet.Domain.Tests/TimeEntries/TimeEntryTests.cs` | создать |
| `Backend/tests/Timesheet.Domain.Tests/Projects/ProjectTests.cs` | создать |
| `Backend/tests/Timesheet.Domain.Tests/PeriodClosures/PeriodClosureTests.cs` | создать |

### Tests — Application

| Файл | Действие |
|---|---|
| `Backend/tests/Timesheet.Application.Tests/TimeEntries/CreateTimeEntryCommandHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/TimeEntries/UpdateTimeEntryCommandHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/TimeEntries/DeleteTimeEntryCommandHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/TimeEntries/ListTimeEntriesQueryHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/TimeEntries/TimeEntryValidatorTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/Employees/ChangeEmployeeRateCommandHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/Employees/RecalculateCostsCommandHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/Employees/ListEmployeesQueryHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/Projects/ListProjectsQueryHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/Reports/ProjectReportQueryHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/PeriodClosures/ClosePeriodCommandHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/PeriodClosures/OpenPeriodCommandHandlerTests.cs` | создать |
| `Backend/tests/Timesheet.Application.Tests/DependencyInjectionTests.cs` | изменить |

### Tests — Infrastructure

| Файл | Действие |
|---|---|
| `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Mappings/BsonMappingTests.cs` | создать |
| `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoTimeEntryRepositoryTests.cs` | создать |
| `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoEmployeeRepositoryTests.cs` | создать |
| `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoProjectRepositoryTests.cs` | создать |
| `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoPeriodClosureRepositoryTests.cs` | создать |
| `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Indexes/IndexCreatorTests.cs` | создать |
| `Backend/tests/Timesheet.Infrastructure.Tests/Fixtures/MongoFixture.cs` | создать |
| `Backend/tests/Timesheet.Infrastructure.Tests/DependencyInjectionTests.cs` | изменить |

### Tests — Api

| Файл | Действие |
|---|---|
| `Backend/tests/Timesheet.Api.Tests/Fixtures/TestWebApplicationFactory.cs` | создать |
| `Backend/tests/Timesheet.Api.Tests/Controllers/TimeEntriesControllerTests.cs` | создать |
| `Backend/tests/Timesheet.Api.Tests/Controllers/EmployeesControllerTests.cs` | создать |
| `Backend/tests/Timesheet.Api.Tests/Controllers/ProjectsControllerTests.cs` | создать |
| `Backend/tests/Timesheet.Api.Tests/Controllers/ReportsControllerTests.cs` | создать |
| `Backend/tests/Timesheet.Api.Tests/Controllers/PeriodsControllerTests.cs` | создать |
| `Backend/tests/Timesheet.Api.Tests/Middleware/ExceptionHandlingMiddlewareTests.cs` | создать |

### Docker / README

| Файл | Действие |
|---|---|
| `Backend/docker-compose.yml` | создать |
| `Backend/README.md` | создать |

### Конфигурация решения

| Файл | Действие |
|---|---|
| `Backend/Directory.Packages.props` | изменить |
| `Backend/Timesheet.sln` | изменить |

---

## Задачи

> **Общее правило TDD:** для каждой задачи с поведенческой логикой:
> 1. `- [ ]` написать failing-тест (red);
> 2. `- [ ]` запустить тест, убедиться в падении;
> 3. `- [ ]` написать минимальную реализацию (green);
> 4. `- [ ]` запустить тест, убедиться в прохождении;
> 5. `- [ ]` закоммитить.
>
> Команды запуска тестов: `dotnet test <project> --configuration Release --no-restore` (или `dotnet test Timesheet.sln --configuration Release` для полного прогона).

---

### Фаза 1. Доменный слой

#### Задача 1.1. Value Objects

**Файлы:**
- `Backend/Timesheet.Domain/Common/EmployeeId.cs`
- `Backend/Timesheet.Domain/Common/ProjectId.cs`
- `Backend/Timesheet.Domain/Common/TimeEntryId.cs`
- `Backend/Timesheet.Domain/Common/DateRange.cs`
- `Backend/tests/Timesheet.Domain.Tests/Common/ValueObjectTests.cs`

**Описание:** четыре `readonly record struct`: `EmployeeId(string Value)`, `ProjectId(string Value)`, `TimeEntryId(string Value)`, `DateRange(DateOnly From, DateOnly To)`. Для `DateRange` — проверка `From <= To` в конструкторе (init-only).

**TDD:**

- [ ] Написать `ValueObjectTests`:
  - `EmployeeId_Equality_SameValue` — два EmployeeId с одинаковым Value равны;
  - `EmployeeId_Inequality_DifferentValue` — с разным Value не равны;
  - `DateRange_Valid_FromLessThanTo` — DateRange(2026-01-01, 2026-01-31) создаётся без исключения;
  - `DateRange_Valid_FromEqualsTo` — DateRange(2026-01-01, 2026-01-01) создаётся без исключения;
  - `DateRange_Invalid_FromGreaterThanTo` — DateRange(2026-02-01, 2026-01-01) выбрасывает `ArgumentException`.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 5 тестов падают (типы не существуют).
- [ ] Создать файлы value objects.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 5 тестов проходят.
- [ ] `git add Backend/Timesheet.Domain/ Backend/tests/Timesheet.Domain.Tests/`
- [ ] `git commit -m "Реализовать value objects доменного слоя"`

**Ожидаемый результат:** 5 passing-тестов, все value objects компилируются.

---

#### Задача 1.2. BusinessException

**Файлы:**
- `Backend/Timesheet.Domain/Common/BusinessException.cs`
- `Backend/tests/Timesheet.Domain.Tests/Common/BusinessExceptionTests.cs`

**Описание:** `BusinessException : Exception` с свойствами `string Code` и `string Message` (переопределено). Конструктор `(string code, string message)`. Используется для всех бизнес-ошибок (PERIOD_CLOSED, DAILY_LIMIT_EXCEEDED и т.д.).

**TDD:**

- [ ] Написать `BusinessExceptionTests`:
  - `Constructor_SetsCodeAndMessage` — new BusinessException("PERIOD_CLOSED", "Период закрыт") имеет Code="PERIOD_CLOSED", Message="Период закрыт";
  - `InheritsFromException` — BusinessException is Exception.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 2 теста падают.
- [ ] Создать `BusinessException.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 2 теста проходят.
- [ ] `git add Backend/Timesheet.Domain/ Backend/tests/Timesheet.Domain.Tests/`
- [ ] `git commit -m "Добавить BusinessException с кодом и сообщением"`

---

#### Задача 1.3. Employee и RateHistoryEntry

**Файлы:**
- `Backend/Timesheet.Domain/Employees/Employee.cs`
- `Backend/Timesheet.Domain/Employees/RateHistoryEntry.cs`
- `Backend/tests/Timesheet.Domain.Tests/Employees/EmployeeTests.cs`

**Описание:**

```csharp
public sealed class Employee
{
    public EmployeeId Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public IReadOnlyList<RateHistoryEntry> RateHistory { get; init; } = [];
    public long RateRevision { get; init; }
}

public sealed record RateHistoryEntry
{
    public DateOnly From { get; init; }
    public decimal Rate { get; init; }
}
```

Инварианты проверяются на уровне приложения (не в конструкторе domain-модели, т.к. объекты создаются из БД):
- `RateHistory` не пуст;
- нет двух записей с одинаковой `From`;
- записи упорядочены по `From` по возрастанию;
- `RateRevision >= 0`.

Метод `GetCurrentRate(DateOnly date)`: найти запись с максимальным `From <= date`, вернуть `Rate`. Если такой записи нет — вернуть ставку с минимальным `From` (самая ранняя).

**TDD:**

- [ ] Написать `EmployeeTests`:
  - `GetCurrentRate_ReturnsRateForDateInRange` — Employee с RateHistory [{From=2026-01-01, Rate=1000}, {From=2026-04-01, Rate=1500}].GetCurrentRate(2026-06-15) == 1500;
  - `GetCurrentRate_ReturnsFirstRate_WhenDateBeforeAllEntries` — Employee с RateHistory [{From=2026-04-01, Rate=1500}].GetCurrentRate(2026-01-01) == 1500 (fallback на самую раннюю);
  - `GetCurrentRate_ReturnsExactMatchRate` — Employee с RateHistory [{From=2026-01-01, Rate=1000}, {From=2026-04-01, Rate=1500}].GetCurrentRate(2026-04-01) == 1500.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 3 теста падают.
- [ ] Создать `Employee.cs` и `RateHistoryEntry.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 3 теста проходят.
- [ ] `git add Backend/Timesheet.Domain/ Backend/tests/Timesheet.Domain.Tests/`
- [ ] `git commit -m "Реализовать Employee и RateHistoryEntry"`

---

#### Задача 1.4. TimeEntry

**Файлы:**
- `Backend/Timesheet.Domain/TimeEntries/TimeEntry.cs`
- `Backend/tests/Timesheet.Domain.Tests/TimeEntries/TimeEntryTests.cs`

**Описание:**

```csharp
public sealed class TimeEntry
{
    public TimeEntryId Id { get; init; }
    public EmployeeId EmployeeId { get; init; }
    public ProjectId ProjectId { get; init; }
    public DateOnly Date { get; init; }
    public decimal Hours { get; init; }
    public string Comment { get; init; } = string.Empty;
    public decimal AppliedRate { get; init; }
    public decimal Cost { get; init; }
    public long RateRevision { get; init; }
    public long Version { get; init; }
}
```

Статический метод `CalculateCost(decimal hours, decimal rate) => Math.Round(hours * rate, 2, MidpointRounding.AwayFromZero)`.

**TDD:**

- [ ] Написать `TimeEntryTests`:
  - `CalculateCost_RoundsCorrectly` — CalculateCost(8.0, 1500.00) == 12000.00;
  - `CalculateCost_RoundsHalfUp` — CalculateCost(1.005m, 1000m) == 1005.00 (проверка MidpointRounding.AwayFromZero);
  - `CalculateCost_SmallHours` — CalculateCost(0.5, 100.00) == 50.00.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 3 теста падают.
- [ ] Создать `TimeEntry.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 3 теста проходят.
- [ ] `git add Backend/Timesheet.Domain/ Backend/tests/Timesheet.Domain.Tests/`
- [ ] `git commit -m "Реализовать TimeEntry с вычислением стоимости"`

---

#### Задача 1.5. Project

**Файлы:**
- `Backend/Timesheet.Domain/Projects/Project.cs`
- `Backend/tests/Timesheet.Domain.Tests/Projects/ProjectTests.cs`

**Описание:**

```csharp
public sealed class Project
{
    public ProjectId Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Budget { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}
```

**TDD:**

- [ ] Написать `ProjectTests`:
  - `Project_CanBeCreated_WithValidData` — new Project { Id = ..., Code = "PRJ-001", Name = "Проект 1", Budget = 2000000 } создаётся без исключения;
  - `Project_CanBeCreated_WithNullDates` — StartDate=null, EndDate=null допустимы.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 2 теста падают.
- [ ] Создать `Project.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 2 теста проходят.
- [ ] `git add Backend/Timesheet.Domain/ Backend/tests/Timesheet.Domain.Tests/`
- [ ] `git commit -m "Реализовать Project"`

---

#### Задача 1.6. PeriodClosure

**Файлы:**
- `Backend/Timesheet.Domain/PeriodClosures/PeriodClosure.cs`
- `Backend/tests/Timesheet.Domain.Tests/PeriodClosures/PeriodClosureTests.cs`

**Описание:**

```csharp
public sealed class PeriodClosure
{
    public int Year { get; init; }
    public int Month { get; init; }
    public bool IsClosed { get; init; }
}
```

**TDD:**

- [ ] Написать `PeriodClosureTests`:
  - `PeriodClosure_CanBeCreated_Closed` — new PeriodClosure { Year=2026, Month=8, IsClosed=true };
  - `PeriodClosure_CanBeCreated_Open` — new PeriodClosure { Year=2026, Month=8, IsClosed=false }.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 2 теста падают.
- [ ] Создать `PeriodClosure.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — 2 теста проходят.
- [ ] `git add Backend/Timesheet.Domain/ Backend/tests/Timesheet.Domain.Tests/`
- [ ] `git commit -m "Реализовать PeriodClosure"`

---

#### Задача 1.7. Верификация доменного слоя

- [ ] `dotnet build Backend/Timesheet.Domain --configuration Release` — сборка без ошибок и предупреждений.
- [ ] `dotnet test Backend/tests/Timesheet.Domain.Tests --configuration Release` — все 15 тестов проходят (5 value objects + 2 exception + 3 employee + 3 time entry + 2 project).

---

### Фаза 2. Application — общая инфраструктура

#### Задача 2.1. ErrorCodes и ErrorMessages

**Файлы:**
- `Backend/Timesheet.Application/Common/Errors/ErrorCodes.cs`
- `Backend/Timesheet.Application/Common/Errors/ErrorMessages.cs`

**Описание:**

`ErrorCodes` — статический класс с константами:
```csharp
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string DailyLimitExceeded = "DAILY_LIMIT_EXCEEDED";
    public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";
    public const string ProjectNotFound = "PROJECT_NOT_FOUND";
    public const string TimeEntryNotFound = "TIME_ENTRY_NOT_FOUND";
    public const string PeriodClosed = "PERIOD_CLOSED";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
}
```

`ErrorMessages` — статический класс с сообщениями на русском:
```csharp
public static class ErrorMessages
{
    public const string DailyLimitExceeded = "Суммарное количество часов за день не может превышать 24";
    public const string EmployeeNotFound = "Сотрудник не найден";
    public const string ProjectNotFound = "Проект не найден";
    public const string TimeEntryNotFound = "Запись табеля не найдена";
    public const string PeriodClosed = "Период закрыт для изменений";
    public const string ConcurrencyConflict = "Конфликт версий: запись была изменена другим пользователем";
}
```

- [ ] Создать оба файла.
- [ ] `dotnet build Backend/Timesheet.Application --configuration Release` — сборка без ошибок.
- [ ] `git add Backend/Timesheet.Application/Common/Errors/`
- [ ] `git commit -m "Добавить коды и сообщения ошибок"`

---

#### Задача 2.2. Repository interfaces (порты)

**Файлы:**
- `Backend/Timesheet.Application/Common/Interfaces/ITimeEntryRepository.cs`
- `Backend/Timesheet.Application/Common/Interfaces/IEmployeeRepository.cs`
- `Backend/Timesheet.Application/Common/Interfaces/IProjectRepository.cs`
- `Backend/Timesheet.Application/Common/Interfaces/IPeriodClosureRepository.cs`
- `Backend/Timesheet.Application/Common/Models/TimeEntryFilter.cs`

**Описание:**

```csharp
public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(TimeEntryId id, CancellationToken ct);
    Task<(IReadOnlyList<TimeEntry> Items, int TotalCount)> ListAsync(TimeEntryFilter filter, CancellationToken ct);
    Task<decimal> SumHoursByEmployeeAndDateAsync(EmployeeId employeeId, DateOnly date, TimeEntryId? excludeId, CancellationToken ct);
    Task<(decimal TotalHours, decimal TotalCost)> SumByFilterAsync(TimeEntryFilter filter, CancellationToken ct);
    Task CreateAsync(TimeEntry entry, CancellationToken ct);
    Task<bool> UpdateAsync(TimeEntry entry, CancellationToken ct);
    Task<bool> DeleteAsync(TimeEntryId id, CancellationToken ct);
    Task UpdateCostsByIntervalAsync(EmployeeId employeeId, DateRange interval, decimal rate, long jobRevision, CancellationToken ct);
}

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(EmployeeId id, CancellationToken ct);
    Task<IReadOnlyList<Employee>> ListAsync(CancellationToken ct);
    Task<long> ChangeRateAsync(EmployeeId id, DateOnly fromDate, decimal newRate, CancellationToken ct);
}

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct);
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<ProjectReportResult>> GetReportsByPeriodAsync(int year, int month, CancellationToken ct);
}

public interface IPeriodClosureRepository
{
    Task<PeriodClosure?> GetAsync(int year, int month, CancellationToken ct);
    Task SetClosedAsync(int year, int month, bool isClosed, CancellationToken ct);
}
```

`TimeEntryFilter`:
```csharp
public sealed record TimeEntryFilter(
    EmployeeId? EmployeeId = null,
    ProjectId? ProjectId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 50);
```

`ProjectReportResult` (в `Common/Models/`):
```csharp
public sealed record ProjectReportResult(
    ProjectId ProjectId,
    string ProjectCode,
    string ProjectName,
    decimal Budget,
    decimal TotalHours,
    decimal TotalCost);
```

- [ ] Создать все файлы интерфейсов и моделей.
- [ ] `dotnet build Backend/Timesheet.Application --configuration Release` — сборка без ошибок.
- [ ] `git add Backend/Timesheet.Application/Common/Interfaces/ Backend/Timesheet.Application/Common/Models/`
- [ ] `git commit -m "Добавить порты репозиториев и модели фильтров"`

---

#### Задача 2.3. Удаление SampleRequest

**Файлы:**
- `Backend/Timesheet.Application/Common/Validation/SampleRequest.cs` — удалить
- `Backend/tests/Timesheet.Application.Tests/DependencyInjectionTests.cs` — изменить

**Описание:** удалить `SampleRequest` и `SampleRequestValidator`. Обновить `DependencyInjectionTests`: заменить тест `AddApplication_RegistersValidatorsFromApplicationAssembly` (который использует `SampleRequest`) на тест, проверяющий регистрацию `ValidationBehavior` через `IPipelineBehavior<,>` (уже есть) и отсутствие `SampleRequest` в assembly.

- [ ] Удалить `SampleRequest.cs`.
- [ ] Обновить `DependencyInjectionTests.cs`: удалить тест `AddApplication_RegistersValidatorsFromApplicationAssembly`, удалить `using Timesheet.Application.Common.Validation`.
- [ ] `dotnet build Backend/Timesheet.Application --configuration Release` — сборка без ошибок.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 2 теста проходят.
- [ ] `git add -A Backend/Timesheet.Application/Common/Validation/ Backend/tests/Timesheet.Application.Tests/DependencyInjectionTests.cs`
- [ ] `git commit -m "Удалить SampleRequest fixture после добавления реальных валидаторов"`

---

### Фаза 3. Application — TimeEntries

#### Задача 3.1. CreateTimeEntry

**Файлы:**
- `Backend/Timesheet.Application/TimeEntries/Create/CreateTimeEntryCommand.cs`
- `Backend/Timesheet.Application/TimeEntries/Create/CreateTimeEntryCommandHandler.cs`
- `Backend/Timesheet.Application/TimeEntries/Create/CreateTimeEntryCommandValidator.cs`
- `Backend/tests/Timesheet.Application.Tests/TimeEntries/CreateTimeEntryCommandHandlerTests.cs`
- `Backend/tests/Timesheet.Application.Tests/TimeEntries/TimeEntryValidatorTests.cs`

**Описание:**

Command:
```csharp
public sealed record CreateTimeEntryCommand(
    string EmployeeId,
    string ProjectId,
    DateOnly Date,
    decimal Hours,
    string Comment) : IRequest<CreateTimeEntryResult>;

public sealed record CreateTimeEntryResult(
    string Id,
    string EmployeeId,
    string ProjectId,
    DateOnly Date,
    decimal Hours,
    string Comment,
    decimal AppliedRate,
    decimal Cost,
    long RateRevision,
    long Version);
```

Validator:
- `EmployeeId` — NotEmpty;
- `ProjectId` — NotEmpty;
- `Date` — не default;
- `Hours` — GreaterThan(0);
- `Comment` — MaxLength(1000).

Handler (псевдокод):
```
1. periodClosure = await periodClosureRepo.GetAsync(command.Date.Year, command.Date.Month, ct)
2. if periodClosure?.IsClosed == true → throw BusinessException(PeriodClosed, ...)
3. employee = await employeeRepo.GetByIdAsync(new EmployeeId(command.EmployeeId), ct)
4. if employee == null → throw BusinessException(EmployeeNotFound, ...)
5. project = await projectRepo.GetByIdAsync(new ProjectId(command.ProjectId), ct)
6. if project == null → throw BusinessException(ProjectNotFound, ...)
7. appliedRate = employee.GetCurrentRate(command.Date)
8. cost = TimeEntry.CalculateCost(command.Hours, appliedRate)
9. sumHours = await timeEntryRepo.SumHoursByEmployeeAndDateAsync(employeeId, command.Date, null, ct)
10. if sumHours + command.Hours > 24 → throw BusinessException(DailyLimitExceeded, ...)
11. entry = new TimeEntry { Id = new TimeEntryId(Guid.NewGuid().ToString()), ..., AppliedRate = appliedRate, Cost = cost, RateRevision = employee.RateRevision, Version = 1 }
12. await timeEntryRepo.CreateAsync(entry, ct)
13. return new CreateTimeEntryResult(...)
```

**TDD:**

- [ ] Написать `TimeEntryValidatorTests`:
  - `Create_WithEmptyEmployeeId_Fails` — EmployeeId="" → validation error;
  - `Create_WithZeroHours_Fails` — Hours=0 → validation error;
  - `Create_WithNegativeHours_Fails` — Hours=-1 → validation error;
  - `Create_WithValidData_Passes` — все поля валидны → 0 errors.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 4 теста падают (валидатор не существует).
- [ ] Создать `CreateTimeEntryCommand.cs`, `CreateTimeEntryCommandValidator.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 4 теста проходят.
- [ ] `git add Backend/Timesheet.Application/TimeEntries/Create/ Backend/tests/Timesheet.Application.Tests/TimeEntries/TimeEntryValidatorTests.cs`
- [ ] `git commit -m "Добавить валидатор CreateTimeEntry"`

- [ ] Написать `CreateTimeEntryCommandHandlerTests` (моки через NSubstitute):
  - `Handle_PeriodClosed_ThrowsBusinessException` — PeriodClosure.IsClosed=true → BusinessException(PeriodClosed);
  - `Handle_EmployeeNotFound_ThrowsBusinessException` — employeeRepo.GetByIdAsync returns null → BusinessException(EmployeeNotFound);
  - `Handle_ProjectNotFound_ThrowsBusinessException` — projectRepo.GetByIdAsync returns null → BusinessException(ProjectNotFound);
  - `Handle_DailyLimitExceeded_ThrowsBusinessException` — sumHours=20, command.Hours=5 → BusinessException(DailyLimitExceeded);
  - `Handle_ValidCommand_CreatesEntry` — все зависимости возвращают валидные данные → CreateTimeEntryResult с корректными AppliedRate, Cost, RateRevision, Version=1;
  - `Handle_ValidCommand_CalculatesCostCorrectly` — Hours=8, Rate=1500 → Cost=12000.00;
  - `Handle_ValidCommand_UsesEmployeeRateRevision` — employee.RateRevision=5 → result.RateRevision=5.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 7 тестов падают (handler не существует).
- [ ] Создать `CreateTimeEntryCommandHandler.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 7 тестов проходят.
- [ ] `git add Backend/Timesheet.Application/TimeEntries/Create/ Backend/tests/Timesheet.Application.Tests/TimeEntries/CreateTimeEntryCommandHandlerTests.cs`
- [ ] `git commit -m "Реализовать CreateTimeEntry command и handler"`

---

#### Задача 3.2. UpdateTimeEntry

**Файлы:**
- `Backend/Timesheet.Application/TimeEntries/Update/UpdateTimeEntryCommand.cs`
- `Backend/Timesheet.Application/TimeEntries/Update/UpdateTimeEntryCommandHandler.cs`
- `Backend/Timesheet.Application/TimeEntries/Update/UpdateTimeEntryCommandValidator.cs`
- `Backend/tests/Timesheet.Application.Tests/TimeEntries/UpdateTimeEntryCommandHandlerTests.cs`

**Описание:**

Command:
```csharp
public sealed record UpdateTimeEntryCommand(
    string Id,
    long Version,
    decimal Hours,
    string Comment) : IRequest<UpdateTimeEntryResult>;

public sealed record UpdateTimeEntryResult(
    string Id,
    string EmployeeId,
    string ProjectId,
    DateOnly Date,
    decimal Hours,
    string Comment,
    decimal AppliedRate,
    decimal Cost,
    long RateRevision,
    long Version);
```

Validator:
- `Id` — NotEmpty;
- `Version` — GreaterThanOrEqualTo(1);
- `Hours` — GreaterThan(0);
- `Comment` — MaxLength(1000).

Handler:
```
1. periodClosure = await periodClosureRepo.GetAsync(existingEntry.Date.Year, existingEntry.Date.Month, ct)
2. if periodClosure?.IsClosed == true → throw BusinessException(PeriodClosed, ...)
3. existingEntry = await timeEntryRepo.GetByIdAsync(new TimeEntryId(command.Id), ct)
4. if existingEntry == null → throw BusinessException(TimeEntryNotFound, ...)
5. if existingEntry.Version != command.Version → throw BusinessException(ConcurrencyConflict, ...)
6. appliedRate = existingEntry.AppliedRate (EmployeeId и Date не меняются при Update)
7. cost = TimeEntry.CalculateCost(command.Hours, appliedRate)
8. sumHours = await timeEntryRepo.SumHoursByEmployeeAndDateAsync(existingEntry.EmployeeId, existingEntry.Date, existingEntry.Id, ct)
9. if sumHours + command.Hours > 24 → throw BusinessException(DailyLimitExceeded, ...)
10. updatedEntry = existingEntry with { Hours = command.Hours, Comment = command.Comment, Cost = cost, Version = existingEntry.Version + 1 }
11. success = await timeEntryRepo.UpdateAsync(updatedEntry, ct)
12. if !success → throw BusinessException(ConcurrencyConflict, ...)
13. return new UpdateTimeEntryResult(...)
```

**Примечание:** Update не меняет EmployeeId и Date, поэтому пересчёт ставки не требуется. AppliedRate остаётся прежним.

**TDD:**

- [ ] Написать `UpdateTimeEntryCommandHandlerTests`:
  - `Handle_EntryNotFound_ThrowsBusinessException` — timeEntryRepo.GetByIdAsync returns null → BusinessException(TimeEntryNotFound);
  - `Handle_VersionMismatch_ThrowsBusinessException` — existingEntry.Version=1, command.Version=2 → BusinessException(ConcurrencyConflict);
  - `Handle_PeriodClosed_ThrowsBusinessException` — PeriodClosure.IsClosed=true → BusinessException(PeriodClosed);
  - `Handle_DailyLimitExceeded_ThrowsBusinessException` — sumHours(excluding current)=20, command.Hours=5 → BusinessException(DailyLimitExceeded);
  - `Handle_ValidCommand_UpdatesEntry` — все OK → UpdateTimeEntryResult с Version=existing.Version+1, Cost пересчитан;
  - `Handle_RepoUpdateFails_ThrowsConcurrencyConflict` — timeEntryRepo.UpdateAsync returns false → BusinessException(ConcurrencyConflict).
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 6 тестов падают.
- [ ] Создать `UpdateTimeEntryCommand.cs`, `UpdateTimeEntryCommandValidator.cs`, `UpdateTimeEntryCommandHandler.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 6 тестов проходят.
- [ ] `git add Backend/Timesheet.Application/TimeEntries/Update/ Backend/tests/Timesheet.Application.Tests/TimeEntries/UpdateTimeEntryCommandHandlerTests.cs`
- [ ] `git commit -m "Реализовать UpdateTimeEntry command и handler"`

---

#### Задача 3.3. DeleteTimeEntry

**Файлы:**
- `Backend/Timesheet.Application/TimeEntries/Delete/DeleteTimeEntryCommand.cs`
- `Backend/Timesheet.Application/TimeEntries/Delete/DeleteTimeEntryCommandHandler.cs`
- `Backend/tests/Timesheet.Application.Tests/TimeEntries/DeleteTimeEntryCommandHandlerTests.cs`

**Описание:**

Command:
```csharp
public sealed record DeleteTimeEntryCommand(string Id) : IRequest;
```

Handler:
```
1. existingEntry = await timeEntryRepo.GetByIdAsync(new TimeEntryId(command.Id), ct)
2. if existingEntry == null → throw BusinessException(TimeEntryNotFound, ...)
3. periodClosure = await periodClosureRepo.GetAsync(existingEntry.Date.Year, existingEntry.Date.Month, ct)
4. if periodClosure?.IsClosed == true → throw BusinessException(PeriodClosed, ...)
5. await timeEntryRepo.DeleteAsync(new TimeEntryId(command.Id), ct)
```

**Примечание:** Delete не проверяет дневную сумму и не использует Version. Проверка закрытого периода выполняется.

**TDD:**

- [ ] Написать `DeleteTimeEntryCommandHandlerTests`:
  - `Handle_EntryNotFound_ThrowsBusinessException` — timeEntryRepo.GetByIdAsync returns null → BusinessException(TimeEntryNotFound);
  - `Handle_PeriodClosed_ThrowsBusinessException` — PeriodClosure.IsClosed=true → BusinessException(PeriodClosed);
  - `Handle_ValidCommand_DeletesEntry` — все OK → timeEntryRepo.DeleteAsync вызван.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 3 теста падают.
- [ ] Создать `DeleteTimeEntryCommand.cs`, `DeleteTimeEntryCommandHandler.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 3 теста проходят.
- [ ] `git add Backend/Timesheet.Application/TimeEntries/Delete/ Backend/tests/Timesheet.Application.Tests/TimeEntries/DeleteTimeEntryCommandHandlerTests.cs`
- [ ] `git commit -m "Реализовать DeleteTimeEntry command и handler"`

---

#### Задача 3.4. ListTimeEntries

**Файлы:**
- `Backend/Timesheet.Application/TimeEntries/List/ListTimeEntriesQuery.cs`
- `Backend/Timesheet.Application/TimeEntries/List/ListTimeEntriesQueryHandler.cs`
- `Backend/tests/Timesheet.Application.Tests/TimeEntries/ListTimeEntriesQueryHandlerTests.cs`

**Описание:**

Query:
```csharp
public sealed record ListTimeEntriesQuery(
    string? EmployeeId = null,
    string? ProjectId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 50) : IRequest<ListTimeEntriesResult>;

public sealed record ListTimeEntriesResult(
    IReadOnlyList<TimeEntryItem> Items,
    int TotalCount,
    int Page,
    int PageSize,
    decimal TotalHours,
    decimal TotalCost);

public sealed record TimeEntryItem(
    string Id,
    string EmployeeId,
    string EmployeeName,
    string ProjectId,
    string ProjectCode,
    DateOnly Date,
    decimal Hours,
    string Comment,
    decimal AppliedRate,
    decimal Cost,
    bool IsOvertime);
```

Handler:
```
1. filter = new TimeEntryFilter(...)
2. (items, totalCount) = await timeEntryRepo.ListAsync(filter, ct)
3. (totalHours, totalCost) = await timeEntryRepo.SumByFilterAsync(filter, ct)
4. Для каждого item: загрузить Employee и Project для получения имени и кода
5. Определить isOvertime: для каждого (EmployeeId, Date) агрегировать Hours за день; если > 12 — все записи этого дня isOvertime=true
6. return new ListTimeEntriesResult(...)
```

**Примечание:** `isOvertime` определяется по суммарным часам сотрудника за календарный день (> 12 часов). Для эффективности можно агрегировать в репозитории.

**TDD:**

- [ ] Написать `ListTimeEntriesQueryHandlerTests`:
  - `Handle_WithNoFilters_ReturnsAllEntries` — репозиторий возвращает 2 записи → result.Items.Count=2, TotalCount=2;
  - `Handle_WithEmployeeFilter_PassesFilterToRepo` — EmployeeId="emp-001" → filter.EmployeeId передан в репозиторий;
  - `Handle_CalculatesTotalHoursAndCost` — 2 записи с Hours=8,Cost=12000 и Hours=4,Cost=6000 → TotalHours=12, TotalCost=18000;
  - `Handle_MarksOvertime_WhenDailyHoursExceed12` — сотрудник с 14 часами за день → IsOvertime=true;
  - `Handle_DoesNotMarkOvertime_WhenDailyHoursLessOrEqual12` — 12 часов за день → IsOvertime=false.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 5 тестов падают.
- [ ] Создать `ListTimeEntriesQuery.cs`, `ListTimeEntriesQueryHandler.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 5 тестов проходят.
- [ ] `git add Backend/Timesheet.Application/TimeEntries/List/ Backend/tests/Timesheet.Application.Tests/TimeEntries/ListTimeEntriesQueryHandlerTests.cs`
- [ ] `git commit -m "Реализовать ListTimeEntries query и handler"`

---

### Фаза 4. Application — Employees

#### Задача 4.1. ChangeEmployeeRate

**Файлы:**
- `Backend/Timesheet.Application/Employees/ChangeRate/ChangeEmployeeRateCommand.cs`
- `Backend/Timesheet.Application/Employees/ChangeRate/ChangeEmployeeRateCommandHandler.cs`
- `Backend/Timesheet.Application/Employees/ChangeRate/ChangeEmployeeRateCommandValidator.cs`
- `Backend/tests/Timesheet.Application.Tests/Employees/ChangeEmployeeRateCommandHandlerTests.cs`

**Описание:**

Command:
```csharp
public sealed record ChangeEmployeeRateCommand(
    string EmployeeId,
    DateOnly FromDate,
    decimal NewRate) : IRequest<ChangeEmployeeRateResult>;

public sealed record ChangeEmployeeRateResult(long NewRateRevision);
```

Validator:
- `EmployeeId` — NotEmpty;
- `NewRate` — GreaterThanOrEqualTo(0);

Handler:
```
1. employee = await employeeRepo.GetByIdAsync(new EmployeeId(command.EmployeeId), ct)
2. if employee == null → throw BusinessException(EmployeeNotFound, ...)
3. newRevision = await employeeRepo.ChangeRateAsync(employeeId, command.FromDate, command.NewRate, ct)
4. return new ChangeEmployeeRateResult(newRevision)
```

**Примечание:** ChangeRate не вызывает RecalculateCosts напрямую — это делает maintenance tool. Handler только атомарно обновляет историю ставок и возвращает новую ревизию.

**TDD:**

- [ ] Написать `ChangeEmployeeRateCommandHandlerTests`:
  - `Handle_EmployeeNotFound_ThrowsBusinessException` — employeeRepo returns null → BusinessException(EmployeeNotFound);
  - `Handle_ValidCommand_ChangesRate` — все OK → employeeRepo.ChangeRateAsync вызван, возвращается новая ревизия;
  - `Handle_ValidCommand_ReturnsNewRevision` — employeeRepo.ChangeRateAsync returns 5 → result.NewRateRevision=5.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 3 теста падают.
- [ ] Создать файлы command, validator, handler.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 3 теста проходят.
- [ ] `git add Backend/Timesheet.Application/Employees/ChangeRate/ Backend/tests/Timesheet.Application.Tests/Employees/ChangeEmployeeRateCommandHandlerTests.cs`
- [ ] `git commit -m "Реализовать ChangeEmployeeRate command и handler"`

---

#### Задача 4.2. RecalculateCosts

**Файлы:**
- `Backend/Timesheet.Application/Employees/RecalculateCosts/RecalculateCostsCommand.cs`
- `Backend/Timesheet.Application/Employees/RecalculateCosts/RecalculateCostsCommandHandler.cs`
- `Backend/tests/Timesheet.Application.Tests/Employees/RecalculateCostsCommandHandlerTests.cs`

**Описание:**

Command:
```csharp
public sealed record RecalculateCostsCommand(
    string EmployeeId,
    long JobRevision) : IRequest;
```

Handler:
```
1. employee = await employeeRepo.GetByIdAsync(new EmployeeId(command.EmployeeId), ct)
2. if employee == null → throw BusinessException(EmployeeNotFound, ...)
3. rateHistory = employee.RateHistory (упорядочен по From)
4. Для каждого i в [0, rateHistory.Count):
   a. from = rateHistory[i].From
   b. to = (i + 1 < rateHistory.Count) ? rateHistory[i+1].From : DateOnly.MaxValue (или достаточно далёкая дата)
   c. rate = rateHistory[i].Rate
   d. interval = new DateRange(from, to)
   e. await timeEntryRepo.UpdateCostsByIntervalAsync(employeeId, interval, rate, command.JobRevision, ct)
```

**Примечание:** `UpdateCostsByIntervalAsync` в репозитории выполняет DB-side `UpdateMany` с aggregation pipeline. Handler не загружает записи в C#. Фильтр `RateRevision < jobRevision` гарантирует идемпотентность и защиту от более старых jobs.

**TDD:**

- [ ] Написать `RecalculateCostsCommandHandlerTests`:
  - `Handle_EmployeeNotFound_ThrowsBusinessException` — employeeRepo returns null → BusinessException(EmployeeNotFound);
  - `Handle_SingleRateEntry_CallsUpdateOnce` — RateHistory имеет 1 запись → timeEntryRepo.UpdateCostsByIntervalAsync вызван 1 раз;
  - `Handle_MultipleRateEntries_CallsUpdateForEachInterval` — RateHistory имеет 3 записи → UpdateCostsByIntervalAsync вызван 3 раза с корректными интервалами;
  - `Handle_PassesJobRevision_ToRepo` — command.JobRevision=5 → передан в UpdateCostsByIntervalAsync.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 4 теста падают.
- [ ] Создать файлы command, handler.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 4 теста проходят.
- [ ] `git add Backend/Timesheet.Application/Employees/RecalculateCosts/ Backend/tests/Timesheet.Application.Tests/Employees/RecalculateCostsCommandHandlerTests.cs`
- [ ] `git commit -m "Реализовать RecalculateCosts command и handler"`

---

#### Задача 4.3. ListEmployees

**Файлы:**
- `Backend/Timesheet.Application/Employees/List/ListEmployeesQuery.cs`
- `Backend/Timesheet.Application/Employees/List/ListEmployeesQueryHandler.cs`
- `Backend/tests/Timesheet.Application.Tests/Employees/ListEmployeesQueryHandlerTests.cs`

**Описание:**

Query:
```csharp
public sealed record ListEmployeesQuery : IRequest<IReadOnlyList<EmployeeListItem>>;

public sealed record EmployeeListItem(
    string Id,
    string FullName,
    decimal CurrentRate);
```

Handler:
```
1. employees = await employeeRepo.ListAsync(ct)
2. return employees.Select(e => new EmployeeListItem(
       e.Id.Value,
       e.FullName,
       e.RateHistory.Count > 0 ? e.RateHistory[^1].Rate : 0m)).ToList()
```

**Примечание:** `CurrentRate` — последняя ставка из RateHistory (RateHistory упорядочен по From, последний элемент — самая свежая ставка).

**TDD:**

- [ ] Написать `ListEmployeesQueryHandlerTests`:
  - `Handle_ReturnsAllEmployees` — employeeRepo returns 2 employees → result.Count=2;
  - `Handle_MapsCurrentRate_FromLastRateHistoryEntry` — Employee с RateHistory [{From=2026-01-01, Rate=1000}, {From=2026-04-01, Rate=1500}] → CurrentRate=1500;
  - `Handle_EmptyList_ReturnsEmpty`.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 3 теста падают.
- [ ] Создать файлы query, handler.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 3 теста проходят.
- [ ] `git add Backend/Timesheet.Application/Employees/List/ Backend/tests/Timesheet.Application.Tests/Employees/ListEmployeesQueryHandlerTests.cs`
- [ ] `git commit -m "Реализовать ListEmployees query и handler"`

---

### Фаза 5. Application — Projects

#### Задача 5.1. ListProjects

**Файлы:**
- `Backend/Timesheet.Application/Projects/List/ListProjectsQuery.cs`
- `Backend/Timesheet.Application/Projects/List/ListProjectsQueryHandler.cs`
- `Backend/tests/Timesheet.Application.Tests/Projects/ListProjectsQueryHandlerTests.cs`

**Описание:**

Query:
```csharp
public sealed record ListProjectsQuery : IRequest<IReadOnlyList<ProjectListItem>>;

public sealed record ProjectListItem(
    string Id,
    string Code,
    string Name,
    decimal Budget,
    DateOnly? StartDate,
    DateOnly? EndDate);
```

Handler:
```
1. projects = await projectRepo.ListAsync(ct)
2. return projects.Select(p => new ProjectListItem(p.Id.Value, p.Code, p.Name, p.Budget, p.StartDate, p.EndDate)).ToList()
```

**TDD:**

- [ ] Написать `ListProjectsQueryHandlerTests`:
  - `Handle_ReturnsAllProjects` — projectRepo returns 2 projects → result.Count=2;
  - `Handle_MapsFields` — все поля корректно маппятся;
  - `Handle_EmptyList_ReturnsEmpty`.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 3 теста падают.
- [ ] Создать файлы query, handler.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 3 теста проходят.
- [ ] `git add Backend/Timesheet.Application/Projects/List/ Backend/tests/Timesheet.Application.Tests/Projects/ListProjectsQueryHandlerTests.cs`
- [ ] `git commit -m "Реализовать ListProjects query и handler"`

---

### Фаза 6. Application — Reports

#### Задача 6.1. ProjectReport (GET /api/reports/projects)

**Файлы:**
- `Backend/Timesheet.Application/Reports/ProjectReport/ProjectReportQuery.cs`
- `Backend/Timesheet.Application/Reports/ProjectReport/ProjectReportQueryHandler.cs`
- `Backend/tests/Timesheet.Application.Tests/Reports/ProjectReportQueryHandlerTests.cs`

**Описание:**

Query:
```csharp
public sealed record ProjectReportQuery(int Year, int Month) : IRequest<IReadOnlyList<ProjectReportItem>>;

public sealed record ProjectReportItem(
    string ProjectId,
    string ProjectCode,
    string ProjectName,
    decimal Budget,
    decimal TotalHours,
    decimal TotalCost,
    decimal UtilizationPercent,
    bool IsAtRisk,
    bool IsOverrun);
```

Handler:
```
1. reports = await projectRepo.GetReportsByPeriodAsync(query.Year, query.Month, ct)
2. return reports.Select(r => {
       utilization = r.Budget > 0 ? Math.Round(r.TotalCost / r.Budget * 100, 2) : 0m;
       return new ProjectReportItem(
           r.ProjectId.Value, r.ProjectCode, r.ProjectName, r.Budget,
           r.TotalHours, r.TotalCost,
           utilization,
           utilization > 80,
           utilization > 100);
   }).ToList()
```

**Примечание:** `GetReportsByPeriodAsync` в репозитории выполняет DB-side aggregation: фильтрация TimeEntries по диапазону дат месяца, группировка по ProjectId, суммирование Hours и Cost, $lookup в Projects.

**TDD:**

- [ ] Написать `ProjectReportQueryHandlerTests`:
  - `Handle_CalculatesUtilizationPercent` — TotalCost=1800000, Budget=2000000 → UtilizationPercent=90.00;
  - `Handle_ZeroBudget_UtilizationPercentIsZero` — Budget=0 → UtilizationPercent=0;
  - `Handle_UtilizationAbove80_IsAtRiskTrue` — UtilizationPercent=90 → IsAtRisk=true;
  - `Handle_UtilizationAbove100_IsOverrunTrue` — UtilizationPercent=110 → IsOverrun=true;
  - `Handle_UtilizationBelow80_NoFlags` — UtilizationPercent=50 → IsAtRisk=false, IsOverrun=false.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 5 тестов падают.
- [ ] Создать файлы query, handler.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 5 тестов проходят.
- [ ] `git add Backend/Timesheet.Application/Reports/ Backend/tests/Timesheet.Application.Tests/Reports/`
- [ ] `git commit -m "Реализовать ProjectReport query и handler"`

---

### Фаза 7. Application — PeriodClosures

#### Задача 7.1. ClosePeriod

**Файлы:**
- `Backend/Timesheet.Application/PeriodClosures/Close/ClosePeriodCommand.cs`
- `Backend/Timesheet.Application/PeriodClosures/Close/ClosePeriodCommandHandler.cs`
- `Backend/tests/Timesheet.Application.Tests/PeriodClosures/ClosePeriodCommandHandlerTests.cs`

**Описание:**

Command:
```csharp
public sealed record ClosePeriodCommand(int Year, int Month) : IRequest<PeriodResult>;

public sealed record PeriodResult(int Year, int Month, bool IsClosed);
```

Handler:
```
1. await periodClosureRepo.SetClosedAsync(command.Year, command.Month, true, ct)
2. return new PeriodResult(command.Year, command.Month, true)
```

**Примечание:** идемпотентно — повторное закрытие не является ошибкой.

**TDD:**

- [ ] Написать `ClosePeriodCommandHandlerTests`:
  - `Handle_CallsSetClosedAsync_WithTrue` — periodClosureRepo.SetClosedAsync вызван с isClosed=true;
  - `Handle_ReturnsCorrectResult` — Year=2026, Month=8 → PeriodResult(2026, 8, true).
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 2 теста падают.
- [ ] Создать файлы command, handler.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 2 теста проходят.
- [ ] `git add Backend/Timesheet.Application/PeriodClosures/Close/ Backend/tests/Timesheet.Application.Tests/PeriodClosures/ClosePeriodCommandHandlerTests.cs`
- [ ] `git commit -m "Реализовать ClosePeriod command и handler"`

---

#### Задача 7.2. OpenPeriod

**Файлы:**
- `Backend/Timesheet.Application/PeriodClosures/Open/OpenPeriodCommand.cs`
- `Backend/Timesheet.Application/PeriodClosures/Open/OpenPeriodCommandHandler.cs`
- `Backend/tests/Timesheet.Application.Tests/PeriodClosures/OpenPeriodCommandHandlerTests.cs`

**Описание:**

Command:
```csharp
public sealed record OpenPeriodCommand(int Year, int Month) : IRequest<PeriodResult>;
```

Handler:
```
1. await periodClosureRepo.SetClosedAsync(command.Year, command.Month, false, ct)
2. return new PeriodResult(command.Year, command.Month, false)
```

**TDD:**

- [ ] Написать `OpenPeriodCommandHandlerTests`:
  - `Handle_CallsSetClosedAsync_WithFalse` — periodClosureRepo.SetClosedAsync вызван с isClosed=false;
  - `Handle_ReturnsCorrectResult` — Year=2026, Month=8 → PeriodResult(2026, 8, false).
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 2 теста падают.
- [ ] Создать файлы command, handler.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — 2 теста проходят.
- [ ] `git add Backend/Timesheet.Application/PeriodClosures/Open/ Backend/tests/Timesheet.Application.Tests/PeriodClosures/OpenPeriodCommandHandlerTests.cs`
- [ ] `git commit -m "Реализовать OpenPeriod command и handler"`

---

#### Задача 7.3. Верификация Application layer

- [ ] `dotnet build Backend/Timesheet.Application --configuration Release` — сборка без ошибок и предупреждений.
- [ ] `dotnet test Backend/tests/Timesheet.Application.Tests --configuration Release` — все unit-тесты Application проходят (ожидаемое количество: 2 DI + 4 validator + 7 create + 6 update + 3 delete + 5 list + 3 change rate + 4 recalculate + 3 list employees + 3 list projects + 5 report + 2 close + 2 open = 51 тест).

---

### Фаза 8. Infrastructure — BSON Mappings

#### Задача 8.1. DateOnlySerializer

**Файлы:**
- `Backend/Timesheet.Infrastructure/MongoDb/Mappings/DateOnlySerializer.cs`
- `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Mappings/BsonMappingTests.cs`

**Описание:** Кастомный `Serializer<DateOnly>` сериализует `DateOnly` как строку `yyyy-MM-dd`.

```csharp
public sealed class DateOnlySerializer : SerializerBase<DateOnly>
{
    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var str = context.Reader.ReadString();
        return DateOnly.ParseExact(str, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
    {
        context.Writer.WriteString(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
```

**TDD:**

- [ ] Написать `BsonMappingTests`:
  - `DateOnlySerializer_RoundTrip` — сериализовать DateOnly(2026,8,25), десериализовать → результат равен исходному;
  - `DateOnlySerializer_SerializesAsString` — результат сериализации — строка "2026-08-25".
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — 2 теста падают.
- [ ] Создать `DateOnlySerializer.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — 2 теста проходят.
- [ ] `git add Backend/Timesheet.Infrastructure/MongoDb/Mappings/DateOnlySerializer.cs Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Mappings/BsonMappingTests.cs`
- [ ] `git commit -m "Реализовать DateOnlySerializer для BSON"`

---

#### Задача 8.2. BsonClassMapConfigurator

**Файлы:**
- `Backend/Timesheet.Infrastructure/MongoDb/Mappings/BsonClassMapConfigurator.cs`

**Описание:** Статический метод `Configure()` регистрирует BsonClassMap для всех document-типов. Вызывается один раз при инициализации Infrastructure (в `AddInfrastructure` или в maintenance tool).

Регистрация:
- `TimeEntryDocument`: Id как string, Date с DateOnlySerializer, Hours/AppliedRate/Cost с Decimal128Serializer;
- `EmployeeDocument`: Id как string, RateHistory элементы с DateOnlySerializer для From, decimal для Rate;
- `ProjectDocument`: Id как string, Budget с Decimal128Serializer, StartDate/EndDate с DateOnlySerializer;
- `PeriodClosureDocument`: составной ключ _id из Year+Month (или отдельный Id).

- [ ] Создать `BsonClassMapConfigurator.cs`.
- [ ] Добавить вызов `BsonClassMapConfigurator.Configure()` в `DependencyInjection.cs` (в `AddInfrastructure`, до создания MongoClient).
- [ ] `dotnet build Backend/Timesheet.Infrastructure --configuration Release` — сборка без ошибок.
- [ ] `git add Backend/Timesheet.Infrastructure/MongoDb/Mappings/BsonClassMapConfigurator.cs Backend/Timesheet.Infrastructure/DependencyInjection.cs`
- [ ] `git commit -m "Добавить BsonClassMapConfigurator и регистрацию маппингов"`

---

#### Задача 8.3. Document models и мапперы

**Файлы:**
- `Backend/Timesheet.Infrastructure/MongoDb/Documents/EmployeeDocument.cs`
- `Backend/Timesheet.Infrastructure/MongoDb/Documents/TimeEntryDocument.cs`
- `Backend/Timesheet.Infrastructure/MongoDb/Documents/ProjectDocument.cs`
- `Backend/Timesheet.Infrastructure/MongoDb/Documents/PeriodClosureDocument.cs`
- `Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/EmployeeMapper.cs`
- `Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/TimeEntryMapper.cs`
- `Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/ProjectMapper.cs`
- `Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/PeriodClosureMapper.cs`

**Описание:** Document-модели — это POCO-классы, отражающие структуру MongoDB-документов. Мапперы конвертируют между domain-моделями и document-моделями (ручной маппинг, без Mapster).

`TimeEntryDocument`:
```csharp
public sealed class TimeEntryDocument
{
    public string Id { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Hours { get; set; }
    public string Comment { get; set; } = string.Empty;
    public decimal AppliedRate { get; set; }
    public decimal Cost { get; set; }
    public long RateRevision { get; set; }
    public long Version { get; set; }
}
```

`EmployeeDocument`:
```csharp
public sealed class EmployeeDocument
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<RateHistoryEntryDocument> RateHistory { get; set; } = [];
    public long RateRevision { get; set; }
}

public sealed class RateHistoryEntryDocument
{
    public DateOnly From { get; set; }
    public decimal Rate { get; set; }
}
```

Мапперы — статические методы `ToDomain(Document)` и `ToDocument(Domain)`.

- [ ] Создать все document-модели и мапперы.
- [ ] `dotnet build Backend/Timesheet.Infrastructure --configuration Release` — сборка без ошибок.
- [ ] `git add Backend/Timesheet.Infrastructure/MongoDb/Documents/ Backend/Timesheet.Infrastructure/MongoDb/DocumentMapping/`
- [ ] `git commit -m "Добавить document-модели и мапперы"`

---

### Фаза 9. Infrastructure — Repositories

#### Задача 9.1. MongoTimeEntryRepository

**Файлы:**
- `Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoTimeEntryRepository.cs`
- `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoTimeEntryRepositoryTests.cs`
- `Backend/tests/Timesheet.Infrastructure.Tests/Fixtures/MongoFixture.cs`

**Описание:** Реализация `ITimeEntryRepository` через `IMongoCollection<TimeEntryDocument>`.

Ключевые методы:
- `GetByIdAsync` — `FindByIdAsync`;
- `ListAsync` — `Find` с фильтром + `Sort` + `Skip/Limit` + `CountDocuments` для TotalCount;
- `SumHoursByEmployeeAndDateAsync` — `Aggregate` с `$match` (EmployeeId, Date, опционально `$ne` Id) + `$group` (`$sum` Hours);
- `SumByFilterAsync` — `Aggregate` с `$match` + `$group` (`$sum` Hours, `$sum` Cost);
- `CreateAsync` — `InsertOneAsync`;
- `UpdateAsync` — `ReplaceOneAsync` с фильтром `{ _id: id, version: expectedVersion }`, возвращает `ModifiedCount > 0`;
- `DeleteAsync` — `DeleteOneAsync` с фильтром `{ _id: id }`, возвращает `DeletedCount > 0`;
- `UpdateCostsByIntervalAsync` — `UpdateManyAsync` с aggregation pipeline:
  ```
  Filter: { employeeId, date: { $gte: from, $lt: to }, rateRevision: { $lt: jobRevision } }
  Pipeline: [{ $set: { appliedRate: rate, cost: { $round: [{ $multiply: ["$hours", rate] }, 2] }, rateRevision: jobRevision } }]
  ```

**MongoFixture** — xUnit collection fixture, поднимает MongoDB через Testcontainers (или использует существующий Docker-контейнер), предоставляет `IMongoDatabase` для тестов.

**TDD (integration):**

- [ ] Создать `MongoFixture` с Testcontainers (MongoDB 7.0).
- [ ] Добавить `Testcontainers.MongoDb` в `Directory.Packages.props`.
- [ ] Написать `MongoTimeEntryRepositoryTests`:
  - `CreateAsync_InsertsDocument` — CreateAsync → GetByIdAsync возвращает вставленную запись;
  - `UpdateAsync_WithCorrectVersion_UpdatesDocument` — UpdateAsync с правильным Version → документ обновлён, Version увеличен;
  - `UpdateAsync_WithWrongVersion_ReturnsFalse` — UpdateAsync с неправильным Version → false, документ не изменён;
  - `DeleteAsync_ExistingEntry_ReturnsTrue` — DeleteAsync → true, GetByIdAsync возвращает null;
  - `DeleteAsync_NonExistingEntry_ReturnsFalse` — DeleteAsync несуществующей записи → false;
  - `SumHoursByEmployeeAndDateAsync_ReturnsSum` — 2 записи с Hours=4 и Hours=3 → sum=7;
  - `SumHoursByEmployeeAndDateAsync_ExcludesId` — с excludeId → сумма без исключённой записи;
  - `ListAsync_WithPagination_ReturnsCorrectPage` — 10 записей, pageSize=3, page=2 → 3 записи, TotalCount=10;
  - `UpdateCostsByIntervalAsync_UpdatesMatchingEntries` — 3 записи в интервале с rateRevision < jobRevision → все обновлены;
  - `UpdateCostsByIntervalAsync_DoesNotUpdateNewerRevision` — запись с rateRevision >= jobRevision → не обновлена.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты падают (репозиторий не существует).
- [ ] Создать `MongoTimeEntryRepository.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoTimeEntryRepository.cs Backend/tests/Timesheet.Infrastructure.Tests/`
- [ ] `git commit -m "Реализовать MongoTimeEntryRepository"`

---

#### Задача 9.2. MongoEmployeeRepository

**Файлы:**
- `Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoEmployeeRepository.cs`
- `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoEmployeeRepositoryTests.cs`

**Описание:** Реализация `IEmployeeRepository`.

Ключевой метод `ChangeRateAsync`:
```javascript
db.employees.findOneAndUpdate(
  { _id: employeeId },
  [
    {
      $set: {
        rateHistory: {
          $sortArray: {
            input: {
              $concatArrays: [
                "$rateHistory",
                [{ from: fromDate, rate: newRate }]
              ]
            },
            sortBy: { from: 1 }
          }
        },
        rateRevision: { $add: ["$rateRevision", 1] }
      }
    }
  ],
  { returnDocument: "after" }
)
```

Возвращает `modifiedDocument.RateRevision`.

**TDD (integration):**

- [ ] Написать `MongoEmployeeRepositoryTests`:
  - `GetByIdAsync_ExistingEmployee_ReturnsEmployee` — seed → GetByIdAsync возвращает Employee;
  - `GetByIdAsync_NonExisting_ReturnsNull` — null;
  - `ListAsync_ReturnsAllEmployees` — seed 3 → ListAsync возвращает 3;
  - `ChangeRateAsync_AddsNewEntry_IncrementsRevision` — ChangeRateAsync → RateHistory содержит новую запись, RateRevision увеличен на 1;
  - `ChangeRateAsync_ReturnsNewRevision` — исходный RateRevision=3 → возвращает 4;
  - `ChangeRateAsync_KeepsHistorySorted` — добавлена запись с более ранней датой → RateHistory упорядочен по From.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты падают.
- [ ] Создать `MongoEmployeeRepository.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoEmployeeRepository.cs Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoEmployeeRepositoryTests.cs`
- [ ] `git commit -m "Реализовать MongoEmployeeRepository"`

---

#### Задача 9.3. MongoProjectRepository

**Файлы:**
- `Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoProjectRepository.cs`
- `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoProjectRepositoryTests.cs`

**Описание:** Реализация `IProjectRepository`.

Метод `GetReportsByPeriodAsync` — DB-side aggregation:
```javascript
db.timeEntries.aggregate([
  { $match: { date: { $gte: fromDate, $lt: toDate } } },
  { $group: { _id: "$projectId", totalHours: { $sum: "$hours" }, totalCost: { $sum: "$cost" } } },
  { $lookup: { from: "projects", localField: "_id", foreignField: "_id", as: "project" } },
  { $unwind: "$project" },
  { $project: {
      projectId: "$_id",
      projectCode: "$project.code",
      projectName: "$project.name",
      budget: "$project.budget",
      totalHours: 1,
      totalCost: 1
  }}
])
```

**TDD (integration):**

- [ ] Написать `MongoProjectRepositoryTests`:
  - `GetByIdAsync_ExistingProject_ReturnsProject`;
  - `GetByIdAsync_NonExisting_ReturnsNull`;
  - `ListAsync_ReturnsAllProjects`;
  - `GetReportsByPeriodAsync_AggregatesByProject` — seed time entries для 2 проектов → возвращает 2 записи с корректными TotalHours и TotalCost;
  - `GetReportsByPeriodAsync_FiltersByDateRange` — записи вне диапазона не включены.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты падают.
- [ ] Создать `MongoProjectRepository.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoProjectRepository.cs Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoProjectRepositoryTests.cs`
- [ ] `git commit -m "Реализовать MongoProjectRepository с aggregation pipeline"`

---

#### Задача 9.4. MongoPeriodClosureRepository

**Файлы:**
- `Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoPeriodClosureRepository.cs`
- `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoPeriodClosureRepositoryTests.cs`

**Описание:** Реализация `IPeriodClosureRepository`.

Метод `SetClosedAsync` — `ReplaceOneAsync` с `IsUpsert=true`. Документ идентифицируется по паре Year+Month (составной _id или отдельный Id = $"{Year}-{Month:D2}").

**TDD (integration):**

- [ ] Написать `MongoPeriodClosureRepositoryTests`:
  - `GetAsync_NonExisting_ReturnsNull`;
  - `SetClosedAsync_CreatesNewDocument` — SetClosedAsync(2026, 8, true) → GetAsync возвращает PeriodClosure { Year=2026, Month=8, IsClosed=true };
  - `SetClosedAsync_UpdatesExistingDocument` — SetClosedAsync(2026, 8, true) → SetClosedAsync(2026, 8, false) → GetAsync возвращает IsClosed=false;
  - `SetClosedAsync_Idempotent` — повторный вызов с тем же значением не вызывает ошибок.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты падают.
- [ ] Создать `MongoPeriodClosureRepository.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Infrastructure/MongoDb/Repositories/MongoPeriodClosureRepository.cs Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Repositories/MongoPeriodClosureRepositoryTests.cs`
- [ ] `git commit -m "Реализовать MongoPeriodClosureRepository"`

---

#### Задача 9.5. Регистрация репозиториев в DI

**Файлы:**
- `Backend/Timesheet.Infrastructure/DependencyInjection.cs` — изменить

**Описание:** Добавить регистрации:
```csharp
services.AddScoped<ITimeEntryRepository, MongoTimeEntryRepository>();
services.AddScoped<IEmployeeRepository, MongoEmployeeRepository>();
services.AddScoped<IProjectRepository, MongoProjectRepository>();
services.AddScoped<IPeriodClosureRepository, MongoPeriodClosureRepository>();
```

- [ ] Обновить `DependencyInjection.cs`.
- [ ] `dotnet build Backend/Timesheet.Infrastructure --configuration Release` — сборка без ошибок.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — все тесты проходят.
- [ ] `git add Backend/Timesheet.Infrastructure/DependencyInjection.cs`
- [ ] `git commit -m "Зарегистрировать репозитории в DI Infrastructure"`

---

### Фаза 10. Infrastructure — Indexes

#### Задача 10.1. IndexCreator

**Файлы:**
- `Backend/Timesheet.Infrastructure/MongoDb/Indexes/IndexCreator.cs`
- `Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Indexes/IndexCreatorTests.cs`

**Описание:** Статический класс с методом `CreateIndexesAsync(IMongoDatabase database, CancellationToken ct)`.

Создаёт индексы:
1. `timeEntries`: `{ employeeId: 1, date: 1 }` — для агрегации часов за день и фильтрации в списках;
2. `periodClosures`: `{ year: 1, month: 1 }` unique — для уникальности пары Year+Month.

**TDD (integration):**

- [ ] Написать `IndexCreatorTests`:
  - `CreateIndexes_CreatesTimeEntryIndex` — после вызова CreateIndexesAsync коллекция timeEntries имеет индекс { employeeId: 1, date: 1 };
  - `CreateIndexes_CreatesPeriodClosureUniqueIndex` — коллекция periodClosures имеет уникальный индекс { year: 1, month: 1 };
  - `CreateIndexes_Idempotent` — повторный вызов не вызывает ошибок.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты падают.
- [ ] Создать `IndexCreator.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Infrastructure.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Infrastructure/MongoDb/Indexes/ Backend/tests/Timesheet.Infrastructure.Tests/MongoDb/Indexes/`
- [ ] `git commit -m "Реализовать IndexCreator для MongoDB"`

---

### Фаза 11. Infrastructure — Maintenance Services

#### Задача 11.1. SeedData

**Файлы:**
- `Backend/Timesheet.Infrastructure/Maintenance/SeedData.cs`

**Описание:** Статический класс с методом `SeedAsync(IMongoDatabase database, CancellationToken ct)`.

Создаёт тестовые данные:
- 3 сотрудника с историей ставок;
- 3 проекта;
- 10-15 записей табеля за август 2026;
- 1 закрытый период (июль 2026).

- [ ] Создать `SeedData.cs`.
- [ ] `dotnet build Backend/Timesheet.Infrastructure --configuration Release` — сборка без ошибок.
- [ ] `git add Backend/Timesheet.Infrastructure/Maintenance/SeedData.cs`
- [ ] `git commit -m "Добавить SeedData для тестовых данных"`

---

#### Задача 11.2. RateChangeService

**Файлы:**
- `Backend/Timesheet.Infrastructure/Maintenance/RateChangeService.cs`

**Описание:** Класс, инкапсулирующий полную операцию изменения ставки:
1. Вызывает `IEmployeeRepository.ChangeRateAsync` — получает новую ревизию;
2. Вызывает `RecalculateCostsCommand` через MediatR (или напрямую `ITimeEntryRepository.UpdateCostsByIntervalAsync` для каждого интервала).

Может использоваться как из maintenance tool, так и как сервис в Infrastructure.

- [ ] Создать `RateChangeService.cs`.
- [ ] `dotnet build Backend/Timesheet.Infrastructure --configuration Release` — сборка без ошибок.
- [ ] `git add Backend/Timesheet.Infrastructure/Maintenance/RateChangeService.cs`
- [ ] `git commit -m "Добавить RateChangeService для изменения ставок"`

---

### Фаза 12. API — Exception Middleware

#### Задача 12.1. ErrorResponse и ExceptionHandlingMiddleware

**Файлы:**
- `Backend/Timesheet.Api/Contracts/ErrorResponse.cs`
- `Backend/Timesheet.Api/Middleware/ExceptionHandlingMiddleware.cs`
- `Backend/tests/Timesheet.Api.Tests/Middleware/ExceptionHandlingMiddlewareTests.cs`

**Описание:**

`ErrorResponse`:
```csharp
public sealed record ErrorResponse(string Code, string Message);
```

`ExceptionHandlingMiddleware`:
- Перехватывает `BusinessException` → маппит в HTTP status и ErrorResponse:
  - `PeriodClosed` → 409;
  - `ConcurrencyConflict` → 409;
  - `DailyLimitExceeded` → 400;
  - `EmployeeNotFound` → 404;
  - `ProjectNotFound` → 404;
  - `TimeEntryNotFound` → 404;
  - default → 400.
- Перехватывает `ValidationException` (FluentValidation) → 400 с `code: "VALIDATION_ERROR"` и `message` — первое сообщение об ошибке;
- Перехватывает все остальные исключения → 500 с `code: "INTERNAL_ERROR"`, `message: "Внутренняя ошибка сервера"` (детали не раскрываются);
- Логирует исключения через `ILogger`.

**TDD:**

- [ ] Написать `ExceptionHandlingMiddlewareTests` (через `HttpContext` mock или `WebApplicationFactory`):
  - `BusinessException_PeriodClosed_Returns409` — BusinessException(PeriodClosed) → 409, body: { code: "PERIOD_CLOSED", message: "Период закрыт для изменений" };
  - `BusinessException_ConcurrencyConflict_Returns409`;
  - `BusinessException_DailyLimitExceeded_Returns400`;
  - `BusinessException_EmployeeNotFound_Returns404`;
  - `ValidationException_Returns400WithValidationError` — ValidationException → 400, body: { code: "VALIDATION_ERROR", message: "..." };
  - `UnhandledException_Returns500` — Exception → 500, body: { code: "INTERNAL_ERROR", message: "Внутренняя ошибка сервера" }.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тесты падают.
- [ ] Создать `ErrorResponse.cs`, `ExceptionHandlingMiddleware.cs`.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Api/Contracts/ErrorResponse.cs Backend/Timesheet.Api/Middleware/ Backend/tests/Timesheet.Api.Tests/Middleware/`
- [ ] `git commit -m "Реализовать ExceptionHandlingMiddleware с форматом ошибок на русском"`

---

### Фаза 13. API — Controllers

#### Задача 13.1. TimeEntriesController

**Файлы:**
- `Backend/Timesheet.Api/Controllers/TimeEntriesController.cs`
- `Backend/Timesheet.Api/Contracts/TimeEntries/CreateTimeEntryRequest.cs`
- `Backend/Timesheet.Api/Contracts/TimeEntries/UpdateTimeEntryRequest.cs`
- `Backend/Timesheet.Api/Contracts/TimeEntries/TimeEntryResponse.cs`
- `Backend/Timesheet.Api/Contracts/TimeEntries/ListTimeEntriesResponse.cs`
- `Backend/tests/Timesheet.Api.Tests/Controllers/TimeEntriesControllerTests.cs`

**Описание:**

Маршруты:
- `POST /api/time-entries` → `CreateTimeEntry`;
- `PUT /api/time-entries/{id}` → `UpdateTimeEntry`;
- `DELETE /api/time-entries/{id}` → `DeleteTimeEntry`;
- `GET /api/time-entries` → `ListTimeEntries`.

Request/Response DTO — отдельные record-типы. Маппинг между DTO и Command/Query — вручную (без Mapster).

`POST /api/time-entries`:
```csharp
[HttpPost]
[ProducesResponseType(typeof(TimeEntryResponse), Status201Created)]
[ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), Status404NotFound)]
[ProducesResponseType(typeof(ErrorResponse), Status409Conflict)]
public async Task<IActionResult> Create([FromBody] CreateTimeEntryRequest request, CancellationToken ct)
{
    var command = new CreateTimeEntryCommand(request.EmployeeId, request.ProjectId, request.Date, request.Hours, request.Comment ?? string.Empty);
    var result = await _mediator.Send(command, ct);
    var response = MapToResponse(result);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, response);
}
```

**TDD (acceptance):**

- [ ] Написать `TimeEntriesControllerTests` (через `WebApplicationFactory` с моками репозиториев):
  - `Create_ValidRequest_Returns201` — POST /api/time-entries с валидными данными → 201, body содержит id, appliedRate, cost, version=1;
  - `Create_InvalidHours_Returns400` — POST с Hours=0 → 400, body: { code: "VALIDATION_ERROR", ... };
  - `Create_PeriodClosed_Returns409` — mock возвращает BusinessException(PeriodClosed) → 409;
  - `Create_EmployeeNotFound_Returns404` — mock возвращает BusinessException(EmployeeNotFound) → 404;
  - `Update_ValidRequest_Returns200` — PUT /api/time-entries/{id} → 200, version увеличен;
  - `Update_VersionMismatch_Returns409` — mock возвращает BusinessException(ConcurrencyConflict) → 409;
  - `Delete_ExistingEntry_Returns204` — DELETE /api/time-entries/{id} → 204;
  - `Delete_NonExisting_Returns404` — mock возвращает BusinessException(TimeEntryNotFound) → 404;
  - `List_WithFilters_Returns200` — GET /api/time-entries?employeeId=emp-001 → 200, body содержит items, totalCount, totalHours, totalCost.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тесты падают.
- [ ] Создать controller, request/response DTOs.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Api/Controllers/TimeEntriesController.cs Backend/Timesheet.Api/Contracts/TimeEntries/ Backend/tests/Timesheet.Api.Tests/Controllers/TimeEntriesControllerTests.cs`
- [ ] `git commit -m "Реализовать TimeEntriesController с CRUD-маршрутами"`

---

#### Задача 13.2. EmployeesController

**Файлы:**
- `Backend/Timesheet.Api/Controllers/EmployeesController.cs`
- `Backend/Timesheet.Api/Contracts/Employees/EmployeeResponse.cs`
- `Backend/tests/Timesheet.Api.Tests/Controllers/EmployeesControllerTests.cs`

**Маршрут:** `GET /api/employees` → `ListEmployees`.

**TDD:**

- [ ] Написать `EmployeesControllerTests`:
  - `List_Returns200_WithEmployees` — GET /api/employees → 200, body содержит массив сотрудников.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тест падает.
- [ ] Создать controller, response DTO.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тест проходит.
- [ ] `git add Backend/Timesheet.Api/Controllers/EmployeesController.cs Backend/Timesheet.Api/Contracts/Employees/ Backend/tests/Timesheet.Api.Tests/Controllers/EmployeesControllerTests.cs`
- [ ] `git commit -m "Реализовать EmployeesController"`

---

#### Задача 13.3. ProjectsController

**Файлы:**
- `Backend/Timesheet.Api/Controllers/ProjectsController.cs`
- `Backend/Timesheet.Api/Contracts/Projects/ProjectResponse.cs`
- `Backend/tests/Timesheet.Api.Tests/Controllers/ProjectsControllerTests.cs`

**Маршрут:** `GET /api/projects` → `ListProjects`.

**TDD:**

- [ ] Написать `ProjectsControllerTests`:
  - `List_Returns200_WithProjects` — GET /api/projects → 200, body содержит массив проектов.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тест падает.
- [ ] Создать controller, response DTO.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тест проходит.
- [ ] `git add Backend/Timesheet.Api/Controllers/ProjectsController.cs Backend/Timesheet.Api/Contracts/Projects/ Backend/tests/Timesheet.Api.Tests/Controllers/ProjectsControllerTests.cs`
- [ ] `git commit -m "Реализовать ProjectsController"`

---

#### Задача 13.4. ReportsController

**Файлы:**
- `Backend/Timesheet.Api/Controllers/ReportsController.cs`
- `Backend/Timesheet.Api/Contracts/Reports/ProjectReportResponse.cs`
- `Backend/tests/Timesheet.Api.Tests/Controllers/ReportsControllerTests.cs`

**Маршрут:** `GET /api/reports/projects?year=&month=` → `ProjectReport`.

**TDD:**

- [ ] Написать `ReportsControllerTests`:
  - `ProjectReport_ValidParams_Returns200` — GET /api/reports/projects?year=2026&month=8 → 200, body содержит массив project report items;
  - `ProjectReport_MissingYear_Returns400` — GET /api/reports/projects?month=8 → 400.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тесты падают.
- [ ] Создать controller, response DTO.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Api/Controllers/ReportsController.cs Backend/Timesheet.Api/Contracts/Reports/ Backend/tests/Timesheet.Api.Tests/Controllers/ReportsControllerTests.cs`
- [ ] `git commit -m "Реализовать ReportsController с маршрутом /api/reports/projects"`

---

#### Задача 13.5. PeriodsController

**Файлы:**
- `Backend/Timesheet.Api/Controllers/PeriodsController.cs`
- `Backend/Timesheet.Api/Contracts/Periods/PeriodRequest.cs`
- `Backend/Timesheet.Api/Contracts/Periods/PeriodResponse.cs`
- `Backend/tests/Timesheet.Api.Tests/Controllers/PeriodsControllerTests.cs`

**Маршруты:**
- `POST /api/periods/close` → `ClosePeriod`;
- `POST /api/periods/open` → `OpenPeriod`.

**TDD:**

- [ ] Написать `PeriodsControllerTests`:
  - `Close_ValidRequest_Returns200` — POST /api/periods/close { year: 2026, month: 8 } → 200, body: { year: 2026, month: 8, isClosed: true };
  - `Open_ValidRequest_Returns200` — POST /api/periods/open { year: 2026, month: 8 } → 200, body: { year: 2026, month: 8, isClosed: false };
  - `Close_Idempotent_Returns200` — повторный POST /api/periods/close → 200.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тесты падают.
- [ ] Создать controller, request/response DTOs.
- [ ] `dotnet test Backend/tests/Timesheet.Api.Tests --configuration Release` — тесты проходят.
- [ ] `git add Backend/Timesheet.Api/Controllers/PeriodsController.cs Backend/Timesheet.Api/Contracts/Periods/ Backend/tests/Timesheet.Api.Tests/Controllers/PeriodsControllerTests.cs`
- [ ] `git commit -m "Реализовать PeriodsController с маршрутами /api/periods/close и /api/periods/open"`

---

#### Задача 13.6. Регистрация middleware в Program.cs

**Файлы:**
- `Backend/Timesheet.Api/Program.cs` — изменить

**Описание:** Добавить `app.UseMiddleware<ExceptionHandlingMiddleware>()` перед `app.MapControllers()`.

- [ ] Обновить `Program.cs`.
- [ ] `dotnet build Backend/Timesheet.Api --configuration Release` — сборка без ошибок.
- [ ] `git add Backend/Timesheet.Api/Program.cs`
- [ ] `git commit -m "Зарегистрировать ExceptionHandlingMiddleware в Program.cs"`

---

### Фаза 14. Maintenance Tool

#### Задача 14.1. Проект Timesheet.Maintenance

**Файлы:**
- `Backend/Timesheet.Maintenance/Timesheet.Maintenance.csproj`
- `Backend/Timesheet.Maintenance/Program.cs`

**Описание:** Консольное приложение .NET 8. Ссылается на `Timesheet.Infrastructure` и `Timesheet.Application`.

`csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <AssemblyName>Timesheet.Maintenance</AssemblyName>
    <RootNamespace>Timesheet.Maintenance</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" />
    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Timesheet.Infrastructure\Timesheet.Infrastructure.csproj" />
    <ProjectReference Include="..\Timesheet.Application\Timesheet.Application.csproj" />
  </ItemGroup>
</Project>
```

Добавить в `Directory.Packages.props`:
```xml
<PackageVersion Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
<PackageVersion Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />
<PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
```

`Program.cs` — парсит аргументы командной строки:
- `seed` — вызывает `SeedData.SeedAsync`;
- `change-rate --employeeId <id> --from <yyyy-MM-dd> --rate <decimal>` — вызывает `RateChangeService`;
- `create-indexes` — вызывает `IndexCreator.CreateIndexesAsync`.

Конфигурация MongoDB читается из `appsettings.json` (или переменных окружения).

- [ ] Добавить пакеты в `Directory.Packages.props`.
- [ ] Создать `Timesheet.Maintenance.csproj`.
- [ ] Создать `Program.cs`.
- [ ] `dotnet sln Backend/Timesheet.sln add Backend/Timesheet.Maintenance/Timesheet.Maintenance.csproj`.
- [ ] `dotnet build Backend/Timesheet.Maintenance --configuration Release` — сборка без ошибок.
- [ ] `git add Backend/Timesheet.Maintenance/ Backend/Directory.Packages.props Backend/Timesheet.sln`
- [ ] `git commit -m "Создать проект Timesheet.Maintenance для seed, изменения ставок и индексов"`

---

### Фаза 15. Docker / README

#### Задача 15.1. Docker Compose

**Файлы:**
- `Backend/docker-compose.yml`

**Описание:**
```yaml
version: '3.8'
services:
  mongo:
    image: mongo:7.0
    container_name: timesheet-mongo
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db

volumes:
  mongo-data:
```

- [ ] Создать `docker-compose.yml`.
- [ ] `git add Backend/docker-compose.yml`
- [ ] `git commit -m "Добавить Docker Compose для standalone MongoDB"`

---

#### Задача 15.2. README

**Файлы:**
- `Backend/README.md`

**Описание:** Документация:
- Предварительные требования (.NET 8 SDK, Docker);
- Запуск MongoDB: `docker-compose up -d`;
- Запуск API: `dotnet run --project Timesheet.Api`;
- Swagger: `http://localhost:5000/swagger`;
- Инициализация базы: `dotnet run --project Timesheet.Maintenance -- create-indexes`;
- Seed данных: `dotnet run --project Timesheet.Maintenance -- seed`;
- Изменение ставки: `dotnet run --project Timesheet.Maintenance -- change-rate --employeeId emp-001 --from 2026-04-01 --rate 1800`;
- Примеры API-запросов (curl);
- Описание маршрутов API;
- Структура решения.

- [ ] Создать `README.md`.
- [ ] `git add Backend/README.md`
- [ ] `git commit -m "Добавить README с инструкциями по запуску и обслуживанию"`

---

### Фаза 16. Финальная верификация

#### Задача 16.1. Полный прогон тестов

- [ ] `dotnet restore Backend/Timesheet.sln`
- [ ] `dotnet build Backend/Timesheet.sln --configuration Release` — сборка без ошибок и предупреждений.
- [ ] `dotnet test Backend/Timesheet.sln --configuration Release --collect:"XPlat Code Coverage"` — все тесты проходят.
- [ ] Проверить code coverage >= 80%.

---

#### Задача 16.2. Статические проверки

**Проверка 1. Отсутствие ORM:**
```bash
grep -r "MongoDB.EntityFrameworkCore\|MongoRepository\|MongoDbGenericRepository" --include="*.csproj" Backend/
```
Ожидаемый результат: вывод пуст.

**Проверка 2. Отсутствие авторизации:**
```bash
grep -r "UseAuthorization\|UseAuthentication\|AddAuthorization\|AddAuthentication" --include="*.cs" Backend/
```
Ожидаемый результат: вывод пуст.

**Проверка 3. Отсутствие Mapster:**
```bash
grep -r "Mapster" --include="*.csproj" --include="*.cs" Backend/
```
Ожидаемый результат: вывод пуст.

**Проверка 4. MongoDB.Driver только в Infrastructure:**
```bash
grep -r "MongoDB.Driver" --include="*.csproj" Backend/Timesheet.Domain/ Backend/Timesheet.Application/ Backend/Timesheet.Api/
```
Ожидаемый результат: вывод пуст.

**Проверка 5. CPM — отсутствие версий в csproj:**
```bash
grep -rn 'PackageReference.*Version=' --include="*.csproj" Backend/
```
Ожидаемый результат: вывод пуст.

**Проверка 6. Отсутствие SampleRequest:**
```bash
grep -r "SampleRequest" --include="*.cs" Backend/
```
Ожидаемый результат: вывод пуст.

**Проверка 7. API startup не выполняет maintenance:**
```bash
grep -rn "SeedData\|CreateIndexes\|RateChangeService" --include="*.cs" Backend/Timesheet.Api/
```
Ожидаемый результат: вывод пуст.

**Проверка 8. Domain не имеет исходящих зависимостей:**
```bash
grep "ProjectReference\|PackageReference" Backend/Timesheet.Domain/Timesheet.Domain.csproj
```
Ожидаемый результат: вывод пуст.

---

#### Задача 16.3. Финальный коммит

- [ ] `git add -A Backend/`
- [ ] `git status` — убедиться, что добавлены только файлы Backend/ (не NOTES.md, LICENSE, code-review/*, Шаблон архитектуры.md, test-task.*).
- [ ] `git commit -m "Реализовать backend Timesheet API" -m "Полная реализация REST API системы учёта рабочего времени: CRUD записей табеля, управление ставками сотрудников, пересчёт стоимости DB-side aggregation, отчёты по проектам, закрытие периодов. Clean Architecture + Vertical Slices, CQRS MediatR, FluentValidation, MongoDB.Driver без ORM. Maintenance tool для seed и изменения ставок. Docker Compose для MongoDB. Покрытие тестами: unit, integration, acceptance."`

---

## Self-review

### Покрытие задач

| Область | Задачи | Статус |
|---|---|---|
| Domain (value objects, entities, exceptions) | 1.1–1.6 | покрыто |
| Application ports | 2.2 | покрыто |
| Application commands/queries | 3.1–3.4, 4.1–4.3, 5.1, 6.1, 7.1–7.2 | покрыто |
| Application validators | 3.1, 3.2, 4.1 | покрыто |
| Удаление SampleRequest | 2.3 | покрыто |
| Infrastructure documents/mappings | 8.1–8.3 | покрыто |
| Infrastructure repositories | 9.1–9.5 | покрыто |
| Infrastructure aggregations | 9.1 (UpdateCostsByIntervalAsync), 9.3 (GetReportsByPeriodAsync) | покрыто |
| Infrastructure indexes | 10.1 | покрыто |
| API controllers | 13.1–13.5 | покрыто |
| API middleware | 12.1, 13.6 | покрыто |
| Maintenance seed | 11.1, 14.1 | покрыто |
| Maintenance rate recalculation | 11.2, 14.1 | покрыто |
| Docker/README | 15.1–15.2 | покрыто |
| Unit tests | в каждой задаче фаз 1–7 | покрыто |
| Integration tests | в каждой задаче фаз 9–10 | покрыто |
| Acceptance tests | в каждой задаче фазы 13 | покрыто |
| Final verification | 16.1–16.3 | покрыто |

### Проверка placeholders

- [x] Нет TBD/TODO в тексте плана.
- [x] Все файлы имеют конкретное содержимое или описание.
- [x] Все команды конкретны и выполнимы.

### Проверка согласованности типов

- [x] `EmployeeId`, `ProjectId`, `TimeEntryId` — `readonly record struct(string Value)` — используются единообразно в Domain, Application, Infrastructure.
- [x] `DateOnly` — везде в API и Domain; сериализуется как `yyyy-MM-dd` через `DateOnlySerializer`.
- [x] `decimal` — везде для денег и часов; сериализуется как `Decimal128` в MongoDB.
- [x] `long RateRevision` — `Employee.RateRevision` и `TimeEntry.RateRevision` — оба `long`.
- [x] `long Version` — `TimeEntry.Version` — `long`, используется для optimistic concurrency.
- [x] `ErrorResponse(Code, Message)` — единообразный формат ошибок во всех контроллерах.
- [x] `BusinessException(Code, Message)` — единообразный источник бизнес-ошибок.

### Проверка маршрутов

| Маршрут | Controller | Метод |
|---|---|---|
| `GET /api/time-entries` | TimeEntriesController | List |
| `POST /api/time-entries` | TimeEntriesController | Create |
| `PUT /api/time-entries/{id}` | TimeEntriesController | Update |
| `DELETE /api/time-entries/{id}` | TimeEntriesController | Delete |
| `GET /api/employees` | EmployeesController | List |
| `GET /api/projects` | ProjectsController | List |
| `GET /api/reports/projects?year=&month=` | ReportsController | ProjectReport |
| `POST /api/periods/close` | PeriodsController | Close |
| `POST /api/periods/open` | PeriodsController | Open |

### Проверка исключённых механизмов

- [x] Нет `daily_totals` (отдельной коллекции/документа для дневных итогов).
- [x] Нет timestamp/time как revision (используется монотонный `RateRevision`).
- [x] Нет cursor для recalculation (используется пакетный UpdateMany по интервалам).
- [x] Нет replica set (standalone MongoDB).
- [x] Нет multi-document transactions (каждый UpdateMany — отдельная операция).
- [x] Нет frontend implementation.
- [x] Нет маршрута `/api/projects/{id}/report`.

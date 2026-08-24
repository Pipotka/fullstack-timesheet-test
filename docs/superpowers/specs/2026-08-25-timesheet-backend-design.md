# Дизайн backend Timesheet API

> Дата: 2026-08-25  
> Статус: утверждён  
> Цель: полная спецификация backend системы учёта рабочего времени

---

## 1. Цель и область применения

### 1.1. Цель

Реализовать REST API для системы учёта рабочего времени (табель) со следующими возможностями:
- создание, изменение и удаление записей табеля;
- хранение истории ставок сотрудников с возможностью изменения задним числом;
- автоматический пересчёт стоимости записей при изменении ставок;
- формирование списков записей с итогами и отчётов по проектам;
- управление закрытыми периодами.

### 1.2. Область применения (Scope)

**Включено:**
- Backend API на .NET 8 с REST-интерфейсом;
- доменная модель, бизнес-логика, валидация;
- хранение данных в MongoDB;
- тесты (unit, integration, acceptance);
- документация API (Swagger/OpenAPI);
- Docker-конфигурация для MongoDB;
- служебные механизмы seed данных и изменения ставок (maintenance tools).

**Исключено:**
- frontend implementation (только описание API-контрактов для будущего UI);
- аутентификация и авторизация (без ролей и пользователей);
- real-time уведомления (WebSocket, SignalR);
- интеграции с внешними системами;
- миграции данных из других систем.

### 1.3. Целевая платформа

- .NET 8 (LTS), C# 12;
- ASP.NET Core Web API;
- MongoDB 7.0+.

---

## 2. Архитектурный подход

### 2.1. Выбранный подход

**Clean Architecture + Vertical Slice Architecture:**
- **Clean Architecture** обеспечивает разделение на слои по ответственности: Domain (ядро), Application (use cases), Infrastructure (адаптеры), Api (точка входа);
- **Vertical Slice Architecture** внутри Application группирует команды и запросы по фичам (вертикальные срезы), а не по техническим папкам;
- **CQRS через MediatR** разделяет операции изменения (Commands) и чтения (Queries);
- **FluentValidation** с pipeline-поведением в MediatR обеспечивает декларативную валидацию.

### 2.2. Альтернативы и обоснование отказа

| Альтернатива | Причина отказа |
|---|---|
| EF Core + реляционная БД | В требованиях указана MongoDB, реляционная БД не обсуждается |
| MongoDB через ORM-обёртку (MongoDB.EntityFrameworkCore, MongoRepository) | ORM скрывает специфику MongoDB, ограничивает доступ к aggregation framework, добавляет ненужную абстракцию |
| Hexagonal / Ports & Adapters как отдельный стиль | По сути совпадает с выбранной Clean Architecture |
| Ручная реализация CQRS без MediatR | Избыточно, потребует написания boilerplate |
| DataAnnotations для валидации | Недостаточно для сложной бизнес-валидации, смешивает concerns |
| Mapster или другой маппер | Не требуется: маппинг выполняется вручную, объём DTO невелик |

---

## 3. Структура решения

### 3.1. Дерево проектов

```
Backend/
├── Directory.Build.props
├── Directory.Packages.props
├── Timesheet.sln
├── .gitignore
│
├── Timesheet.Domain/
│   ├── Employees/
│   │   ├── Employee.cs
│   │   ├── RateHistoryEntry.cs
│   │   └── EmployeeId.cs
│   ├── Projects/
│   │   ├── Project.cs
│   │   └── ProjectId.cs
│   ├── TimeEntries/
│   │   ├── TimeEntry.cs
│   │   └── TimeEntryId.cs
│   ├── PeriodClosures/
│   │   └── PeriodClosure.cs
│   └── Common/
│       ├── Money.cs
│       └── DateRange.cs
│
├── Timesheet.Application/
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs
│   │   ├── Interfaces/
│   │   │   ├── ITimeEntryRepository.cs
│   │   │   ├── IEmployeeRepository.cs
│   │   │   ├── IProjectRepository.cs
│   │   │   └── IPeriodClosureRepository.cs
│   │   └── Errors/
│   │       └── ErrorCodes.cs
│   ├── TimeEntries/
│   │   ├── Create/
│   │   │   ├── CreateTimeEntryCommand.cs
│   │   │   └── CreateTimeEntryCommandHandler.cs
│   │   ├── Update/
│   │   │   ├── UpdateTimeEntryCommand.cs
│   │   │   └── UpdateTimeEntryCommandHandler.cs
│   │   ├── Delete/
│   │   │   ├── DeleteTimeEntryCommand.cs
│   │   │   └── DeleteTimeEntryCommandHandler.cs
│   │   ├── List/
│   │   │   ├── ListTimeEntriesQuery.cs
│   │   │   └── ListTimeEntriesQueryHandler.cs
│   │   └── Validators/
│   │       └── TimeEntryValidators.cs
│   ├── Employees/
│   │   ├── ChangeRate/
│   │   │   ├── ChangeEmployeeRateCommand.cs
│   │   │   └── ChangeEmployeeRateCommandHandler.cs
│   │   └── RecalculateCosts/
│   │       ├── RecalculateCostsCommand.cs
│   │       └── RecalculateCostsCommandHandler.cs
│   ├── Projects/
│   │   └── Report/
│   │       ├── ProjectReportQuery.cs
│   │       └── ProjectReportQueryHandler.cs
│   ├── PeriodClosures/
│   │   ├── Close/
│   │   │   ├── ClosePeriodCommand.cs
│   │   │   └── ClosePeriodCommandHandler.cs
│   │   └── Open/
│   │       ├── OpenPeriodCommand.cs
│   │       └── OpenPeriodCommandHandler.cs
│   └── DependencyInjection.cs
│
├── Timesheet.Infrastructure/
│   ├── MongoDb/
│   │   ├── MongoDbSettings.cs
│   │   ├── MongoDbServiceCollectionExtensions.cs
│   │   ├── Repositories/
│   │   │   ├── MongoTimeEntryRepository.cs
│   │   │   ├── MongoEmployeeRepository.cs
│   │   │   ├── MongoProjectRepository.cs
│   │   │   └── MongoPeriodClosureRepository.cs
│   │   ├── Mappings/
│   │   │   └── BsonClassMapConfigurator.cs
│   │   └── Indexes/
│   │       └── IndexCreator.cs
│   ├── Maintenance/
│   │   ├── SeedData.cs
│   │   └── RateChangeService.cs
│   └── DependencyInjection.cs
│
├── Timesheet.Api/
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── TimeEntriesController.cs
│   │   ├── ProjectsController.cs
│   │   └── PeriodClosuresController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Properties/
│       └── launchSettings.json
│
└── tests/
    ├── Timesheet.Domain.Tests/
    ├── Timesheet.Application.Tests/
    ├── Timesheet.Infrastructure.Tests/
    └── Timesheet.Api.Tests/
```

### 3.2. Dependency graph

```
Timesheet.Domain          ← ни от кого не зависит (ядро)
Timesheet.Application     → Timesheet.Domain
Timesheet.Infrastructure  → Timesheet.Application, Timesheet.Domain
Timesheet.Api             → Timesheet.Application, Timesheet.Infrastructure

Timesheet.Domain.Tests       → Timesheet.Domain
Timesheet.Application.Tests  → Timesheet.Application
Timesheet.Infrastructure.Tests → Timesheet.Infrastructure
Timesheet.Api.Tests          → Timesheet.Api
```

**Принципы:**
- Domain не имеет исходящих project references — это гарантирует независимость ядра;
- Application определяет порты (интерфейсы репозиториев), Infrastructure реализует адаптеры;
- Api не ссылается на Domain напрямую (зависимость приходит транзитивно через Application);
- тестовые проекты ссылаются только на тот проект, который тестируют.

---

## 4. Доменная модель

### 4.1. Employee (Сотрудник)

```csharp
public sealed class Employee
{
    public EmployeeId Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public IReadOnlyList<RateHistoryEntry> RateHistory { get; init; } = [];
    public long RateRevision { get; private set; }
}

public sealed record RateHistoryEntry
{
    public DateOnly From { get; init; }
    public decimal Rate { get; init; }
}
```

**Инварианты:**
- `RateHistory` не пуст (минимум одна ставка);
- `RateHistory` не содержит двух записей с одинаковой датой `From`;
- `RateRevision` монотонно возрастает (целое число, увеличивается через `$inc`);
- ставки упорядочены по `From` по возрастанию.

### 4.2. TimeEntry (Запись табеля)

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

**Инварианты:**
- `Hours > 0`;
- `AppliedRate >= 0`;
- `Cost = round(Hours * AppliedRate, 2)`;
- `RateRevision` — снимок ревизии расчёта на момент вычисления `AppliedRate` и `Cost`;
- `Version` используется для optimistic concurrency при Update.

### 4.3. Project (Проект)

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

**Инварианты:**
- `Budget >= 0`;
- если указаны `StartDate` и `EndDate`, то `StartDate <= EndDate`.

### 4.4. PeriodClosure (Закрытие периода)

```csharp
public sealed class PeriodClosure
{
    public int Year { get; init; }
    public int Month { get; init; }
    public bool IsClosed { get; init; }
}
```

**Инварианты:**
- `Year > 0`, `Month` от 1 до 12;
- `IsClosed` определяет, заблокирован ли период для изменений.

### 4.5. Value Objects

```csharp
public readonly record struct EmployeeId(string Value);
public readonly record struct ProjectId(string Value);
public readonly record struct TimeEntryId(string Value);
public readonly record struct DateRange(DateOnly From, DateOnly To);
```

---

## 5. API Endpoints

### 5.1. TimeEntries

#### POST `/api/time-entries` — создание записи

**Request:**
```json
{
  "employeeId": "emp-001",
  "projectId": "proj-001",
  "date": "2026-08-25",
  "hours": 8.0,
  "comment": "Разработка модуля"
}
```

**Response (201 Created):**
```json
{
  "id": "entry-001",
  "employeeId": "emp-001",
  "projectId": "proj-001",
  "date": "2026-08-25",
  "hours": 8.0,
  "comment": "Разработка модуля",
  "appliedRate": 1500.00,
  "cost": 12000.00,
  "rateRevision": 5,
  "version": 1
}
```

**Errors:**
- `400 Bad Request` — валидация (некорректные данные, `hours <= 0`);
- `400 Bad Request` с кодом `DAILY_LIMIT_EXCEEDED` — суммарно за день больше 24 часов;
- `409 Conflict` с кодом `PERIOD_CLOSED` — период закрыт;
- `404 Not Found` с кодом `EMPLOYEE_NOT_FOUND` или `PROJECT_NOT_FOUND`.

**Логика:**
1. Проверить, что период не закрыт;
2. Проверить, что сотрудник и проект существуют;
3. Определить действующую ставку на дату записи (из `RateHistory`);
4. Вычислить `Cost = round(Hours * AppliedRate, 2)`;
5. Агрегировать часы за день для сотрудника, проверить `<= 24`;
6. Сохранить запись с `RateRevision` из `Employee.RateRevision`.

#### PUT `/api/time-entries/{id}` — изменение записи

**Request:**
```json
{
  "version": 1,
  "hours": 7.5,
  "comment": "Обновлённый комментарий"
}
```

**Response (200 OK):**
```json
{
  "id": "entry-001",
  "employeeId": "emp-001",
  "projectId": "proj-001",
  "date": "2026-08-25",
  "hours": 7.5,
  "comment": "Обновлённый комментарий",
  "appliedRate": 1500.00,
  "cost": 11250.00,
  "rateRevision": 5,
  "version": 2
}
```

**Errors:**
- `400 Bad Request` — валидация;
- `400 Bad Request` с кодом `DAILY_LIMIT_EXCEEDED` — суммарно за день больше 24 часов;
- `409 Conflict` с кодом `PERIOD_CLOSED` — период закрыт;
- `409 Conflict` с кодом `CONCURRENCY_CONFLICT` — версия не совпадает (optimistic concurrency);
- `404 Not Found` с кодом `TIME_ENTRY_NOT_FOUND`.

**Логика:**
1. Проверить, что период не закрыт;
2. Загрузить запись, проверить `Version` из запроса совпадает с `Version` в БД;
3. Если ставка изменилась (дата или сотрудник), пересчитать `AppliedRate` и `Cost`;
4. Агрегировать часы за день (без текущей записи), проверить `<= 24`;
5. Сохранить с `Version + 1`.

#### DELETE `/api/time-entries/{id}` — удаление записи

**Request:** нет тела (или `{ "id": "entry-001" }` в route).

**Response (204 No Content).**

**Errors:**
- `409 Conflict` с кодом `PERIOD_CLOSED` — период закрыт;
- `404 Not Found` с кодом `TIME_ENTRY_NOT_FOUND`.

**Логика:**
1. Проверить, что период не закрыт;
2. Удалить запись (без проверки дневной суммы, без `Version`).

#### GET `/api/time-entries` — список записей

**Query parameters:**
- `employeeId` (optional);
- `projectId` (optional);
- `fromDate` (optional, `yyyy-MM-dd`);
- `toDate` (optional, `yyyy-MM-dd`);
- `page` (default: 1);
- `pageSize` (default: 50, max: 200).

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "entry-001",
      "employeeId": "emp-001",
      "employeeName": "Иванов Иван Иванович",
      "projectId": "proj-001",
      "projectCode": "PRJ-001",
      "date": "2026-08-25",
      "hours": 8.0,
      "comment": "Разработка модуля",
      "appliedRate": 1500.00,
      "cost": 12000.00,
      "isOvertime": false
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 50,
  "totalHours": 1200.5,
  "totalCost": 1800750.00
}
```

**Логика:**
- `isOvertime` — признак переработки: если суммарно за день у сотрудника больше 12 часов, все записи этого дня помечаются;
- `totalHours` и `totalCost` — итоги по всем записям, подходящим под фильтры (не только по текущей странице);
- DB-side aggregation для подсчёта итогов.

### 5.2. Employees

#### GET `/api/employees` — список сотрудников

**Response (200 OK):**
```json
[
  {
    "id": "emp-001",
    "fullName": "Иванов Иван Иванович",
    "currentRate": 1500.00
  }
]
```

**Логика:**
- возвращает всех сотрудников с текущей ставкой (последняя запись в `RateHistory`).

### 5.3. Projects

#### GET `/api/projects` — список проектов

**Response (200 OK):**
```json
[
  {
    "id": "proj-001",
    "code": "PRJ-001",
    "name": "Проект 1",
    "budget": 2000000.00,
    "startDate": "2026-01-01",
    "endDate": "2026-12-31"
  }
]
```

### 5.4. Reports

#### GET `/api/reports/projects?year=&month=` — отчёты по проектам за период

**Query parameters:**
- `year` (required, integer);
- `month` (required, integer, 1–12).

**Response (200 OK):**
```json
[
  {
    "projectId": "proj-001",
    "projectCode": "PRJ-001",
    "projectName": "Проект 1",
    "budget": 2000000.00,
    "totalHours": 1200.5,
    "totalCost": 1800750.00,
    "utilizationPercent": 90.04,
    "isAtRisk": false,
    "isOverrun": false
  }
]
```

**Логика:**
- `utilizationPercent = (totalCost / budget) * 100`;
- если `budget == 0`, то `utilizationPercent = 0`;
- `isAtRisk = utilizationPercent > 80`;
- `isOverrun = utilizationPercent > 100`;
- DB-side aggregation для `totalHours` и `totalCost`.

### 5.5. Periods

#### POST `/api/periods/close` — закрыть период

**Request:**
```json
{
  "year": 2026,
  "month": 8
}
```

**Response (200 OK):**
```json
{
  "year": 2026,
  "month": 8,
  "isClosed": true
}
```

**Логика:**
- идемпотентно: повторное закрытие не является ошибкой.

#### POST `/api/periods/open` — открыть период

**Request:**
```json
{
  "year": 2026,
  "month": 8
}
```

**Response (200 OK):**
```json
{
  "year": 2026,
  "month": 8,
  "isClosed": false
}
```

**Логика:**
- идемпотентно: повторное открытие не является ошибкой.

### 5.6. Данные для будущего UI

API возвращает все данные, необходимые для интерфейса:
- **Список записей:** `id`, `employeeName`, `projectCode`, `date`, `hours`, `comment`, `appliedRate`, `cost`, `isOvertime`;
- **Пагинация:** `totalCount`, `page`, `pageSize`;
- **Итоги:** `totalHours`, `totalCost`;
- **Список сотрудников:** `id`, `fullName`, `currentRate`;
- **Список проектов:** `id`, `code`, `name`, `budget`, `startDate`, `endDate`;
- **Отчёты по проектам:** `projectId`, `projectCode`, `projectName`, `budget`, `totalHours`, `totalCost`, `utilizationPercent`, `isAtRisk`, `isOverrun`;
- **Ошибки:** `{ code, message }` на русском языке.

---

## 6. Application Layer

### 6.1. Commands и Queries

**Commands (изменение состояния):**
- `CreateTimeEntryCommand` → `CreateTimeEntryCommandHandler`;
- `UpdateTimeEntryCommand` → `UpdateTimeEntryCommandHandler`;
- `DeleteTimeEntryCommand` → `DeleteTimeEntryCommandHandler`;
- `ChangeEmployeeRateCommand` → `ChangeEmployeeRateCommandHandler`;
- `RecalculateCostsCommand` → `RecalculateCostsCommandHandler`;
- `ClosePeriodCommand` → `ClosePeriodCommandHandler`;
- `OpenPeriodCommand` → `OpenPeriodCommandHandler`.

**Queries (чтение):**
- `ListTimeEntriesQuery` → `ListTimeEntriesQueryHandler`;
- `ProjectReportQuery` → `ProjectReportQueryHandler`.

### 6.2. Ports (интерфейсы репозиториев)

```csharp
public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(TimeEntryId id, CancellationToken ct);
    Task<IReadOnlyList<TimeEntry>> ListAsync(TimeEntryFilter filter, CancellationToken ct);
    Task<decimal> SumHoursByEmployeeAndDateAsync(EmployeeId employeeId, DateOnly date, CancellationToken ct);
    Task CreateAsync(TimeEntry entry, CancellationToken ct);
    Task UpdateAsync(TimeEntry entry, CancellationToken ct);
    Task DeleteAsync(TimeEntryId id, CancellationToken ct);
    Task UpdateCostsAsync(EmployeeId employeeId, DateRange range, decimal rate, long newRevision, CancellationToken ct);
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

### 6.3. Validation

FluentValidation с pipeline-поведением `ValidationBehavior<TRequest, TResponse>`:
- валидаторы выполняются автоматически перед обработчиком;
- ошибки валидации возвращаются с кодом `400 Bad Request` и телом `{ code: "VALIDATION_ERROR", message: "..." }`.

---

## 7. Workflows

### 7.1. Create TimeEntry

1. **Валидация:** `CreateTimeEntryCommandValidator` проверяет `employeeId`, `projectId`, `date`, `hours > 0`;
2. **Проверка периода:** загрузить `PeriodClosure` для месяца записи, если `IsClosed == true` → ошибка `PERIOD_CLOSED`;
3. **Загрузка сотрудника:** получить `Employee`, если не найден → ошибка `EMPLOYEE_NOT_FOUND`;
4. **Определение ставки:** найти в `RateHistory` запись с максимальной `From <= Date`, получить `Rate`;
5. **Вычисление стоимости:** `Cost = round(Hours * Rate, 2)`;
6. **Проверка дневного лимита:** агрегировать `Hours` за `Date` для `EmployeeId`, если `sum + Hours > 24` → ошибка `DAILY_LIMIT_EXCEEDED`;
7. **Сохранение:** создать `TimeEntry` с `AppliedRate = Rate`, `Cost`, `RateRevision = Employee.RateRevision`, `Version = 1`;
8. **Возврат:** `201 Created` с данными записи.

### 7.2. Update TimeEntry

1. **Валидация:** `UpdateTimeEntryCommandValidator` проверяет `version`, `hours > 0`;
2. **Проверка периода:** загрузить `PeriodClosure`, если `IsClosed == true` → ошибка `PERIOD_CLOSED`;
3. **Загрузка записи:** получить `TimeEntry` по `id`, если не найден → ошибка `TIME_ENTRY_NOT_FOUND`;
4. **Optimistic concurrency:** если `entry.Version != command.Version` → ошибка `CONCURRENCY_CONFLICT`;
5. **Пересчёт ставки (если нужно):** если изменился `EmployeeId` или `Date`, определить новую ставку, пересчитать `AppliedRate` и `Cost`;
6. **Проверка дневного лимита:** агрегировать `Hours` за `Date` для `EmployeeId` (без текущей записи), если `sum + newHours > 24` → ошибка `DAILY_LIMIT_EXCEEDED`;
7. **Сохранение:** обновить `TimeEntry` с `Version + 1`, фильтр по `Id` и `Version`;
8. **Возврат:** `200 OK` с обновлёнными данными.

### 7.3. Delete TimeEntry

1. **Проверка периода:** загрузить `PeriodClosure`, если `IsClosed == true` → ошибка `PERIOD_CLOSED`;
2. **Удаление:** удалить `TimeEntry` по `id` (без проверки дневной суммы, без `Version`);
3. **Возврат:** `204 No Content`.

---

## 8. Stored Cost и Rate Mutation

### 8.1. Хранение стоимости

- в `TimeEntry` хранятся `AppliedRate` и `Cost` (оба `decimal` / MongoDB `Decimal128`);
- списки и отчёты читают сохранённые значения, динамический пересчёт при чтении не выполняется;
- это обеспечивает производительность и согласованность данных.

### 8.2. Изменение ставки

**Механизм (maintenance tool, не публичный REST):**
1. Приложение определяет новую ставку и дату начала действия;
2. Атомарная операция в MongoDB:
   - добавить новую запись в `Employee.RateHistory`;
   - упорядочить `RateHistory` по `From`;
   - `$inc Employee.RateRevision` на 1;
3. Операция возвращает новую ревизию (`RateRevision`);
4. Эта ревизия используется для пересчёта стоимости записей.

### 8.3. Пересчёт стоимости (DB-side)

**Алгоритм:**
1. Приложение определяет интервалы `[from, nextRateFrom)` для каждой ставки;
2. Для каждого интервала выполняется `UpdateMany` с aggregation update pipeline:
   ```javascript
   db.timeEntries.updateMany(
     {
       employeeId: "emp-001",
       date: { $gte: ISODate("2026-01-01"), $lt: ISODate("2026-04-01") },
       rateRevision: { $lt: 5 }
     },
     [
       {
         $set: {
           appliedRate: 1500.00,
           cost: { $round: [{ $multiply: ["$hours", 1500.00] }, 2] },
           rateRevision: 5
         }
       }
     ]
   )
   ```
3. Фильтр `RateRevision < jobRevision` гарантирует, что более старый job не перезаписывает данные, уже обновленные более новой ревизией;
4. миллионы записей не грузятся в C#, всё выполняется на стороне MongoDB;
5. диапазон дат сужается индексом `EmployeeId + Date`;
6. при необходимости диапазон разбивается на месячные или недельные чанки;
7. каждый чанк обновляется отдельным `UpdateMany` без единой огромной транзакции;
8. частичный прогресс допустим, безопасный повтор устраняет незавершённые обновления.

**Инварианты:**
- пересчёт идемпотентен;
- повторный запуск безопасен;
- конкурентные пересчёты не конфликтуют благодаря фильтру `RateRevision < jobRevision`.

---

## 9. MongoDB BSON Mappings

### 9.1. DateOnly

- `DateOnly` сериализуется как строка в формате `yyyy-MM-dd`;
- пример: `2026-08-25`;
- кастомный `Serializer<DateOnly>` регистрируется в `BsonClassMapConfigurator`.

### 9.2. Decimal

- `decimal` сериализуется как `Decimal128`;
- пример: `1500.00`, `12000.00`;
- стандартный `Decimal128Serializer` используется из `MongoDB.Bson.Serialization.Serializers`.

### 9.3. Пример BsonClassMap

```csharp
BsonClassMap.RegisterClassMap<TimeEntry>(cm =>
{
    cm.MapIdProperty(e => e.Id);
    cm.SetSerializer(new StringSerializer(BsonType.String)); // для Id
    cm.MapProperty(e => e.Date).SetSerializer(new DateOnlySerializer());
    cm.MapProperty(e => e.Hours).SetSerializer(new Decimal128Serializer());
    cm.MapProperty(e => e.AppliedRate).SetSerializer(new Decimal128Serializer());
    cm.MapProperty(e => e.Cost).SetSerializer(new Decimal128Serializer());
});
```

---

## 10. Индексы MongoDB

### 10.1. TimeEntries

```csharp
collection.Indexes.CreateOne(new CreateIndexModel<TimeEntry>(
    Builders<TimeEntry>.IndexKeys
        .Ascending(e => e.EmployeeId)
        .Ascending(e => e.Date)
));
```

**Назначение:**
- ускорение агрегации часов за день для проверки дневного лимита;
- ускорение фильтрации по сотруднику и дате в списках;
- сужение диапазона для пересчёта стоимости.

### 10.2. PeriodClosures

```csharp
collection.Indexes.CreateOne(new CreateIndexModel<PeriodClosure>(
    Builders<PeriodClosure>.IndexKeys
        .Ascending(p => p.Year)
        .Ascending(p => p.Month),
    new CreateIndexOptions { Unique = true }
));
```

**Назначение:**
- уникальный индекс по паре `Year + Month`;
- ускорение проверки закрытости периода.

### 10.3. Создание индексов

- индексы создаются явной командой (maintenance tool), не при старте приложения;
- startup API не выполняет seed, создание индексов или сетевые вызовы;
- для standalone MongoDB индексы создаются один раз при инициализации базы.

---

## 11. Ошибки и Status Mapping

### 11.1. Формат ошибок

```json
{
  "code": "DAILY_LIMIT_EXCEEDED",
  "message": "Суммарное количество часов за день не может превышать 24"
}
```

- `code` — машиночитаемый код ошибки (верхний регистр, snake_case);
- `message` — сообщение на русском языке.

### 11.2. Status mapping

| Код ошибки | HTTP Status | Описание |
|---|---|---|
| `VALIDATION_ERROR` | `400 Bad Request` | Ошибка валидации входных данных |
| `DAILY_LIMIT_EXCEEDED` | `400 Bad Request` | Превышен дневной лимит часов |
| `EMPLOYEE_NOT_FOUND` | `404 Not Found` | Сотрудник не найден |
| `PROJECT_NOT_FOUND` | `404 Not Found` | Проект не найден |
| `TIME_ENTRY_NOT_FOUND` | `404 Not Found` | Запись табеля не найдена |
| `PERIOD_CLOSED` | `409 Conflict` | Период закрыт для изменений |
| `CONCURRENCY_CONFLICT` | `409 Conflict` | Конфликт версий (optimistic concurrency) |

### 11.3. ExceptionHandlingMiddleware

- перехватывает исключения из обработчиков;
- преобразует бизнес-исключения в соответствующие HTTP-статусы и форматы ошибок;
- логирует исключения через `ILogger`.

---

## 12. DI и Composition Root

### 12.1. Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
```

### 12.2. AddApplication

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    var assembly = typeof(DependencyInjection).Assembly;

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    services.AddValidatorsFromAssembly(assembly);
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    return services;
}
```

### 12.3. AddInfrastructure

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var settings = configuration
        .GetSection("MongoDb")
        .Get<MongoDbSettings>()
        ?? throw new InvalidOperationException("MongoDb settings are missing");

    services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(settings.ConnectionString));

    services.AddSingleton<IMongoDatabase>(sp =>
        sp.GetRequiredService<IMongoClient>()
          .GetDatabase(settings.DatabaseName));

    services.AddScoped<ITimeEntryRepository, MongoTimeEntryRepository>();
    services.AddScoped<IEmployeeRepository, MongoEmployeeRepository>();
    services.AddScoped<IProjectRepository, MongoProjectRepository>();
    services.AddScoped<IPeriodClosureRepository, MongoPeriodClosureRepository>();

    return services;
}
```

---

## 13. Maintenance и Seed

### 13.1. Seed данных

- seed выполняется отдельным maintenance tool (консольное приложение или скрипт);
- не выполняется при старте API;
- создаёт тестовые данные: сотрудники, проекты, записи табеля, закрытые периоды.

### 13.2. Изменение ставки

- изменение ставки выполняется через `RateChangeService` (maintenance tool);
- не является публичным REST endpoint;
- описывается в `README` как служебная операция.

### 13.3. Создание индексов

- индексы создаются явной командой при инициализации базы;
- не выполняются при старте API.

---

## 14. Тесты

### 14.1. Unit Tests (Timesheet.Domain.Tests)

- тесты доменных инвариантов (валидация `RateHistory`, `Hours > 0`, `Cost` calculation);
- тесты value objects.

### 14.2. Unit Tests (Timesheet.Application.Tests)

- тесты команд и запросов (handlers);
- тесты валидаторов;
- тесты pipeline-поведений;
- моки репозиториев через NSubstitute.

### 14.3. Integration Tests (Timesheet.Infrastructure.Tests)

- тесты репозиториев с реальной MongoDB (через Testcontainers или in-memory заглушки);
- тесты BSON mappings;
- тесты пересчёта стоимости.

### 14.4. Acceptance Tests (Timesheet.Api.Tests)

- тесты контроллеров через `WebApplicationFactory`;
- тесты end-to-end сценариев (create → update → delete);
- тесты optimistic concurrency;
- тесты закрытых периодов.

### 14.5. Фреймворки

- xUnit;
- FluentAssertions;
- NSubstitute (для моков);
- Microsoft.AspNetCore.Mvc.Testing (для acceptance tests);
- coverlet.collector (для code coverage).

---

## 15. Docker и README

### 15.1. Docker Compose

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

**Назначение:**
- MongoDB для разработки и тестирования.

### 15.2. README

- описание запуска API (`dotnet run`);
- описание подключения к MongoDB (connection string);
- описание seed данных (maintenance tool);
- описание изменения ставки (maintenance tool);
- описание создания индексов (maintenance tool);
- примеры API-запросов (curl или Postman).

---

## 16. Ограничения и допущения

### 16.1. Concurrency

- конкурентным считается только редактирование одной и той же записи табеля (TimeEntry);
- для Update используется optimistic concurrency по паре `Id + Version`;
- Create и Delete не требуют передачи `Version`;
- конкурентные Create или Update разных записей одного сотрудника за один день не получают строгой защиты дневного лимита: проверка выполняется последовательностью `aggregate → check → write` без транзакционной изоляции;
- это осознанное ограничение тестового задания, строгая защита параллельных Create или разных Update не входит в scope.

### 16.2. Stored Cost

- в записи табеля хранятся `AppliedRate` и `Cost`;
- списки и отчёты читают сохранённые значения, динамический пересчёт при чтении не выполняется;
- при изменении истории ставок служебный механизм пересчитывает `AppliedRate` и `Cost` во всех затронутых записях;
- повторный пересчёт идемпотентен;
- существует риск частичного пересчёта при сбое, который устраняется безопасным повторным запуском.

### 16.3. RateRevision

- `Employee.RateRevision` — монотонный логический счётчик (целое число);
- увеличивается атомарно через `$inc` в той же операции обновления, которая изменяет `RateHistory`;
- `TimeEntry.RateRevision` — снимок ревизии расчёта, действовавшей на момент вычисления `AppliedRate` и `Cost`;
- скрипт пересчёта получает новую ревизию как результат атомарной операции `$inc` на `Employee.RateRevision`;
- ревизия берётся из результата операции, не из max `TimeEntry` и не из времени.

### 16.4. Упрощения

- итоги списков и отчётов вычисляются DB-side aggregation на лету;
- ревизия определяется монотонным счётчиком, не временем;
- пересчёт выполняется пакетно по диапазонам дат, не по одной записи;
- достаточно standalone MongoDB.

### 16.5. Другие допущения

- закрытый период блокирует создание, изменение и удаление записей табеля, но не историю ставок;
- изменение ставки задним числом допустимо;
- итоги по отфильтрованному списку вычисляются по всем записям, подходящим под фильтры, а не только по текущей странице;
- переработка определяется по суммарному количеству часов сотрудника за календарный день (> 12 часов);
- бюджет проекта считается положительным, если `budget == 0`, то `utilizationPercent = 0`;
- все даты в системе — календарные даты без времени, формат `yyyy-MM-dd`.

---

## 17. Критерии приёмки

### 17.1. Функциональные требования

- [ ] создание записи табеля с вычислением стоимости;
- [ ] изменение записи с optimistic concurrency;
- [ ] удаление записи без проверки дневной суммы;
- [ ] блокировка изменений закрытым периодом;
- [ ] список записей с итогами и признаком переработки;
- [ ] отчёт по проекту с процентом освоения бюджета;
- [ ] закрытие и открытие периодов (идемпотентно);
- [ ] изменение ставки сотрудника с пересчётом стоимости;
- [ ] пересчёт стоимости DB-side aggregation pipeline.

### 17.2. Нефункциональные требования

- [ ] все тесты проходят (unit, integration, acceptance);
- [ ] code coverage >= 80%;
- [ ] API документирован через Swagger;
- [ ] Docker Compose для standalone MongoDB;
- [ ] README с инструкциями по запуску и обслуживанию;
- [ ] нет незаполненных разделов, временных заглушек и противоречий в коде и документации;
- [ ] ошибки возвращаются в формате `{ code, message }` на русском языке.

### 17.3. Ограничения

- [ ] нет аутентификации и авторизации;
- [ ] нет strict protection для конкурентных Create или разных Update.

---

## 18. Резюме

Спецификация описывает backend систему учёта рабочего времени на .NET 8 с Clean Architecture + Vertical Slice Architecture, CQRS через MediatR, FluentValidation, официальным MongoDB.Driver без ORM.

**Ключевые решения:**
- stored cost (`AppliedRate`, `Cost` в `TimeEntry`);
- monotonic `RateRevision` для отслеживания изменений ставок;
- DB-side recalculation через aggregation update pipeline;
- optimistic concurrency для Update (`Id + Version`);
- maintenance tools для seed и изменения ставок (не публичный REST).

Спецификация пригодна для одного последующего implementation plan.

# Timesheet Backend

Backend для учёта рабочего времени сотрудников по проектам. .NET 8, MongoDB 7, Clean Architecture.

## Быстрый старт (5 шагов)

Все команды выполняются из корня репозитория.

### 1. Запустить MongoDB

```bash
docker compose -f Backend/docker-compose.yml up -d
```

Проверка готовности:

```bash
docker compose -f Backend/docker-compose.yml ps
```

Подключение: `mongodb://localhost:27017`, база данных `Timesheet`.

### 2. Создать индексы

```bash
dotnet run --project Backend/Timesheet.Maintenance -- indexes
```

### 3. Загрузить начальные данные

```bash
dotnet run --project Backend/Timesheet.Maintenance -- seed
```

### 4. Запустить API

```bash
dotnet run --project Backend/Timesheet.Api
```

Swagger: http://localhost:5000/swagger (или порт из `Properties/launchSettings.json`).

### 5. Проверить работу

Отчёт за март 2026:

```bash
curl "http://localhost:5000/api/reports/projects?year=2026&month=3"
```

Ожидаемый результат: P-001 — 12h / 7600 / 38%, P-002 — 10h / 7000 / 140% (перерасход), итого 22h / 14600.

Отчёт за февраль 2026:

```bash
curl "http://localhost:5000/api/reports/projects?year=2026&month=2"
```

Ожидаемый результат: 8h / 4000 / 20%.

## Изменение ставки сотрудника

```bash
dotnet run --project Backend/Timesheet.Maintenance -- change-rate --employee seed-employee-ivanov --from 2026-03-01 --rate 650
```

Команда атомарно увеличивает `RateRevision` сотрудника и пересчитывает `AppliedRate`/`Cost` во всех затронутых записях табеля через `UpdateMany` на стороне MongoDB. Повторный запуск с теми же параметрами не увеличивает ревизию повторно, если ставка не изменилась.

После изменения ставки 600 -> 650 за март 2026 для Иванова: запись за 2026-03-10 (8h) будет пересчитана: 8 * 650 = 5200.

## Переопределение конфигурации

Подключение к MongoDB задаётся в `Backend/Timesheet.Api/appsettings.json` и может быть переопределено переменными окружения:

```bash
export MongoDb__ConnectionString=mongodb://host:27017
export MongoDb__DatabaseName=Timesheet
```

Для Maintenance tool можно создать `Backend/Timesheet.Maintenance/appsettings.Maintenance.json`:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "Timesheet"
  }
}
```

## Коллекции и индексы

| Коллекция | Индексы |
|-----------|---------|
| `time_entries` | `idx_time_entries_employee_date` (EmployeeId, Date), `idx_time_entries_project_date` (ProjectId, Date), `idx_time_entries_date_id` (Date, Id) |
| `projects` | `idx_projects_code_unique` (Code, unique) |
| `period_closures` | `idx_period_closures_year_month_unique` (Year, Month, unique) |
| `employees` | `idx_employees_fullname` (FullName) |

## Хранение стоимости и пересчёт

В каждой записи табеля хранятся `AppliedRate` (применённая ставка) и `Cost` (стоимость = Hours * AppliedRate, округление до копеек). Отчёты и списки читают сохранённые значения, динамический пересчёт при чтении не выполняется.

При изменении ставки служебная команда пересчитывает `AppliedRate`, `Cost` и `RateRevision` во всех затронутых записях через `UpdateMany` с aggregation pipeline. Записи фильтруются по `RateRevision < jobRevision`, что защищает от перезаписи более новым пересчётом.

## Известные ограничения

- **Дневной лимит часов (24h)** проверяется последовательностью aggregate -> check -> write без транзакционной изоляции. Конкурентные Create/Update разных записей одного сотрудника за один день не получают строгой защиты лимита. Это осознанное ограничение тестового задания.
- **Конкурентные пересчёты стоимости** безопасны: более старый job не перезаписывает данные, обновлённые более новой ревизией.
- **Нет аутентификации/авторизации** — тестовое задание.
- **Нет публичного эндпоинта управления ставками** — изменение ставки выполняется через Maintenance tool.
- **При старте API индексы не создаются, данные не загружаются** — это явное требование.

## Сборка и тесты

```bash
dotnet restore Backend/Timesheet.sln
dotnet build Backend/Timesheet.sln --configuration Release
dotnet test Backend/Timesheet.sln --configuration Release
```

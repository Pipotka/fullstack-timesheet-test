# Дизайн структуры backend Timesheet

> Дата: 2026-08-24
> Статус: утверждён
> Цель этапа: каркас и базовая инфраструктура backend без бизнес-логики

---

## 1. Контекст

Репозиторий `fullstack-timesheet-test` содержит тестовое задание (табель учёта рабочего времени) и результаты code review. На момент старта этапа backend содержит только минимальный шаблонный проект `Backend/Timesheet.Api` (один веб-проект с контроллерами, Swagger и `UseAuthorization`), сгенерированный стандартным шаблоном `dotnet new webapi`.

Документ `NOTES.md` фиксирует принятые допущения по спецификации:

- календарные даты без времени, формат `yyyy-MM-dd`;
- изменение ставки задним числом допустимо и не блокируется закрытыми периодами;
- стоимость записи вычисляется динамически на основе ставки, действующей на дату записи;
- конкурентное редактирование через версию записи (optimistic concurrency);
- REST-методы: `POST /api/time-entries` (создание), `PUT /api/time-entries/{id}` (изменение);
- ошибки бизнес-правил возвращаются с кодом состояния 400/409 и телом `{ code, message }`.

Шаблон `Шаблон архитектуры.md` в корне репозитория рассматривается как отправная точка для адаптации, а не как источник для прямого копирования. Структура, описанная в данном документе, адаптирует идеи шаблона под конкретные требования проекта Timesheet.

---

## 2. Цели и границы этапа

### 2.1. Цели

1. Создать скелет решения в Clean Architecture с вертикальными срезами (Vertical Slice Architecture) как организационным принципом внутри слоя Application.
2. Настроить централизованное управление версиями пакетов (Central Package Management) через `Directory.Packages.props` и общие свойства сборки через `Directory.Build.props`.
3. Подключить MediatR и FluentValidation в слое Application с pipeline-поведением `ValidationBehavior<TRequest, TResponse>`.
4. Подключить официальный `MongoDB.Driver` исключительно в слое Infrastructure, зарегистрировать `IMongoClient` и `IMongoDatabase` через DI.
5. Обеспечить корректную DI-конфигурацию всех слоёв в `Program.cs`.
6. Настроить тестовые проекты (xUnit + FluentAssertions) с правильными project references.
7. Убрать из стартового шаблона `UseAuthorization` и любые заготовки авторизации, так как авторизация не входит в рамки текущего этапа.

### 2.2. Границы (что НЕ делается на этом этапе)

- Бизнес-сущности (Employee, Project, TimeEntry, EmployeeRate, PeriodClosure и т. д.) не определяются.
- Endpoint'ы контроллеров не реализуются.
- Commands и Queries (MediatR-обработчики) не создаются.
- Репозитории не реализуются.
- Seed данных не выполняется.
- Индексы MongoDB не создаются.
- Бизнес-тесты не пишутся.
- Mapster (или любой другой маппер) не подключается.
- Авторизация и аутентификация не реализуются.
- Ping MongoDB, миграции, seed и создание индексов при старте приложения не выполняются.

---

## 3. Рассмотренные варианты и принятое решение

### 3.1. Архитектурный стиль

| Вариант | Обоснование |
|---|---|
| Классическая Clean Architecture (слои по ответственности) | Принят как основа: чёткое разделение Domain / Application / Infrastructure / Api обеспечивает тестируемость и независимость от фреймворков. |
| Vertical Slice Architecture как дополнение | Внутри слоя Application команды и запросы организуются по фичам (vertical slices), а не по техническим папкам. Это снижает связность внутри среза и упрощает навигацию. |
| Hexagonal / Ports & Adapters | По сути совпадает с выбранной Clean Architecture: Domain и Application определяют порты (интерфейсы репозиториев), Infrastructure реализует адаптеры (MongoDB). |

**Решение:** Clean Architecture как основа организации проектов + Vertical Slice как принцип группировки команд/запросов внутри Application.

### 3.2. Хранилище данных

| Вариант | Обоснование |
|---|---|
| EF Core + реляционная БД | Не подходит: в требованиях указана MongoDB, реляционная БД не обсуждается. |
| MongoDB через ORM-обёртку (MongoDB.EntityFrameworkCore / MongoRepository) | Отклонён: ORM-обёртки скрывают специфику MongoDB, добавляют ненужную абстракцию и ограничивают доступ к aggregation framework. |
| Официальный `MongoDB.Driver` напрямую | Принят: прямой доступ к `IMongoDatabase`, `IMongoCollection<T>`, aggregation pipeline; никаких generic-абстракций поверх MongoDB. |

**Решение:** `MongoDB.Driver` напрямую в Infrastructure, без ORM, без DbContext, без generic Mongo abstraction.

### 3.3. CQRS и медиатор

| Вариант | Обоснование |
|---|---|
| Ручная реализация CQRS | Избыточно для текущего этапа, потребует написания boilerplate. |
| MediatR | Принят: стандартный выбор для CQRS в .NET-экосистеме, хорошо интегрируется с pipeline-поведениями (валидация, логирование). |

**Решение:** MediatR для CQRS в слое Application.

### 3.4. Валидация

| Вариант | Обоснование |
|---|---|
| DataAnnotations | Недостаточно для сложной бизнес-валидации, смешивает concerns. |
| FluentValidation + MediatR pipeline | Принят: декларативные валидаторы, автоматическое выполнение через `ValidationBehavior<TRequest, TResponse>`, единый формат ошибок. |

**Решение:** FluentValidation с pipeline-поведением в MediatR.

### 3.5. Маппинг

| Вариант | Обоснование |
|---|---|
| Mapster | Отложен: на этапе каркаса нет сущностей и DTO, маппинг не требуется. Подключение будет выполнено на этапе реализации бизнес-логики, если потребуется. |
| Ручной маппинг | Достаточно для начального этапа, но пока не нужен. |

**Решение:** Mapster не добавляется на этом этапе.

---

## 4. Структура решения

### 4.1. Дерево файлов и проектов

```
Backend/
├── Directory.Build.props
├── Directory.Packages.props
├── Timesheet.sln
├── .gitignore
│
├── Timesheet.Domain/
│   └── Timesheet.Domain.csproj
│
├── Timesheet.Application/
│   ├── Timesheet.Application.csproj
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs
│   │   └── Interfaces/
│   │       └── (порты репозиториев — на следующем этапе)
│   └── DependencyInjection.cs
│
├── Timesheet.Infrastructure/
│   ├── Timesheet.Infrastructure.csproj
│   ├── MongoDb/
│   │   └── MongoDbServiceCollectionExtensions.cs
│   └── DependencyInjection.cs
│
├── Timesheet.Api/
│   ├── Timesheet.Api.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Properties/
│       └── launchSettings.json
│
└── tests/
    ├── Timesheet.Domain.Tests/
    │   └── Timesheet.Domain.Tests.csproj
    ├── Timesheet.Application.Tests/
    │   └── Timesheet.Application.Tests.csproj
    ├── Timesheet.Infrastructure.Tests/
    │   └── Timesheet.Infrastructure.Tests.csproj
    └── Timesheet.Api.Tests/
        └── Timesheet.Api.Tests.csproj
```

### 4.2. Описание проектов

| Проект | Тип SDK | Назначение |
|---|---|---|
| `Timesheet.Domain` | `Microsoft.NET.Sdk` (class library) | Ядро: бизнес-сущности, value objects, доменные события, интерфейсы репозиториев (порты). На данном этапе — пустой проект-заготовка. |
| `Timesheet.Application` | `Microsoft.NET.Sdk` (class library) | Use cases: команды, запросы, валидаторы, pipeline-поведения, интерфейсы внешних сервисов. Содержит `DependencyInjection.cs` для регистрации MediatR и `ValidationBehavior`. |
| `Timesheet.Infrastructure` | `Microsoft.NET.Sdk` (class library) | Адаптеры: реализация портов из Application/Domain, конфигурация MongoDB.Driver, регистрации `IMongoClient`/`IMongoDatabase`. |
| `Timesheet.Api` | `Microsoft.NET.Sdk.Web` | Точка входа: `Program.cs`, DI-композиция, middleware, контроллеры (на следующем этапе). |
| `Timesheet.Domain.Tests` | `Microsoft.NET.Sdk` (xUnit) | Тесты доменного слоя. |
| `Timesheet.Application.Tests` | `Microsoft.NET.Sdk` (xUnit) | Тесты use cases, валидаторов, pipeline-поведений. |
| `Timesheet.Infrastructure.Tests` | `Microsoft.NET.Sdk` (xUnit) | Тесты инфраструктуры (integration-тесты с MongoDB потребуют testcontainers или in-memory заглушки — на следующем этапе). |
| `Timesheet.Api.Tests` | `Microsoft.NET.Sdk` (xUnit) | Тесты контроллеров и middleware через `WebApplicationFactory`. |

---

## 5. Зависимости между проектами (Project References)

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

Принципы:

- Тестовые проекты ссылаются только на тот проект, который тестируют (и его транзитивные зависимости). Кросс-ссылки между тестовыми проектами запрещены.
- Api не ссылается на Domain напрямую (зависимость приходит транзитивно через Application).
- Infrastructure ссылается на Application (для реализации портов) и на Domain (для работы с сущностями).
- Domain не имеет исходящих project references — это гарантирует независимость ядра.

---

## 6. Централизованное управление пакетами

### 6.1. `Directory.Build.props`

Располагается в корне `Backend/`. Задаёт общие свойства для всех проектов решения:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Deterministic>true</Deterministic>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
```

Ключевые решения:

- `TargetFramework` вынесен на уровень решения — все проекты используют `net8.0`.
- `Nullable` и `ImplicitUsings` включены глобально — не дублируются в отдельных `.csproj`.
- `Deterministic` обеспечивает воспроизводимые сборки.
- `TreatWarningsAsErrors` предотвращает накопление предупреждений.
- `ManagePackageVersionsCentrally` включает CPM.

### 6.2. `Directory.Packages.props`

Располагается в корне `Backend/`. Содержит все версии NuGet-пакетов в одном месте:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Application -->
    <PackageVersion Include="MediatR" Version="12.4.1" />
    <PackageVersion Include="FluentValidation" Version="11.11.0" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />

    <!-- Infrastructure -->
    <PackageVersion Include="MongoDB.Driver" Version="3.1.0" />

    <!-- Api -->
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="6.6.2" />

    <!-- Tests -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="FluentAssertions" Version="7.0.0" />
    <PackageVersion Include="NSubstitute" Version="5.3.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.4" />
  </ItemGroup>
</Project>
```

Отдельные `.csproj`-файлы ссылаются на пакеты без указания версии:

```xml
<!-- Пример: Timesheet.Application.csproj -->
<ItemGroup>
  <PackageReference Include="MediatR" />
  <PackageReference Include="FluentValidation" />
  <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
</ItemGroup>
```

---

## 7. Пакеты по проектам

| Проект | NuGet-пакеты |
|---|---|
| `Timesheet.Domain` | _(нет внешних пакетов)_ |
| `Timesheet.Application` | `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions` |
| `Timesheet.Infrastructure` | `MongoDB.Driver`, `Microsoft.Extensions.DependencyInjection.Abstractions` |
| `Timesheet.Api` | `Swashbuckle.AspNetCore`, project references на Application и Infrastructure |
| `Timesheet.Domain.Tests` | `xunit`, `FluentAssertions`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector` |
| `Timesheet.Application.Tests` | `xunit`, `FluentAssertions`, `NSubstitute`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector` |
| `Timesheet.Infrastructure.Tests` | `xunit`, `FluentAssertions`, `NSubstitute`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector` |
| `Timesheet.Api.Tests` | `xunit`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector` |

---

## 8. Dependency Injection

### 8.1. Регистрация в слоях

Каждой слой предоставляет статический метод расширения `IServiceCollection` для регистрации своих сервисов:

- `Timesheet.Application.DependencyInjection.AddApplication(this IServiceCollection services)` — регистрирует MediatR (сканирование assembly) и `ValidationBehavior` как open-generic transient.
- `Timesheet.Infrastructure.DependencyInjection.AddInfrastructure(this IServiceCollection services)` — регистрирует MongoDB-клиент и базу данных, а на следующем этапе — реализации репозиториев.

### 8.2. Композиция в `Program.cs`

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
app.MapControllers();

app.Run();
```

Ключевые отличия от стартового шаблона:

- `app.UseAuthorization()` удалён — авторизация не входит в рамки этапа.
- Добавлены вызовы `AddApplication()` и `AddInfrastructure()`.

### 8.3. Конфигурация MongoDB

Настройки подключения вынесены в `appsettings.json` в секцию `MongoDb`:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "Timesheet"
  }
}
```

В Infrastructure определён класс `MongoDbSettings` (record), связанный с этой секцией через `IOptions<MongoDbSettings>`. Валидация обязательности `ConnectionString` и `DatabaseName` выполняется при старте через `OptionsValidator` или явную проверку в `AddInfrastructure`.

---

## 9. MongoDB в Infrastructure

### 9.1. Принципы

- Используется только официальный пакет `MongoDB.Driver`.
- `IMongoClient` регистрируется как singleton (рекомендация производителя — один клиент на приложение).
- `IMongoDatabase` регистрируется как singleton, получается через `IMongoClient.GetDatabase(settings.DatabaseName)`.
- Репозитории (на следующем этапе) будут получать `IMongoDatabase` через DI и вызывать `database.GetCollection<T>("collectionName")`.
- Никаких generic-абстракций поверх MongoDB (типа `IMongoRepository<T>`) не создаётся.
- Никакого ORM (MongoDB.EntityFrameworkCore и аналогов) не используется.

### 9.2. Регистрация

```csharp
public static IServiceCollection AddMongoDb(
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

    return services;
}
```

### 9.3. Что НЕ делается при старте

- Нет ping-проверки подключения к MongoDB.
- Нет создания индексов.
- Нет миграций.
- Нет seed данных.

Эти операции будут реализованы на этапе бизнес-логики как отдельные hosted services или явные вызовы, если это потребуется.

---

## 10. MediatR и валидация в Application

### 10.1. Регистрация MediatR

```csharp
public static IServiceCollection AddApplication(
    this IServiceCollection services)
{
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

    services.AddTransient(typeof(IPipelineBehavior<,>),
        typeof(ValidationBehavior<,>));

    return services;
}
```

### 10.2. ValidationBehavior

```csharp
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}
```

`ValidationException` определяется в Application и на следующем этапе будет обработана global exception filter / middleware в Api для возврата ответа формата `{ code, message }` с кодом состояния 400, как указано в `NOTES.md`.

---

## 11. Тестовая инфраструктура

### 11.1. Фреймворки

- **xUnit** — тестовый раннер и атрибуты.
- **FluentAssertions** — читаемые ассерты.
- **NSubstitute** — моки и заглушки (для Application.Tests и Infrastructure.Tests).
- **Microsoft.AspNetCore.Mvc.Testing** — `WebApplicationFactory` для Api.Tests.
- **coverlet** — покрытие кода.

### 11.2. Структура тестовых проектов

Каждый тестовый проект содержит пустой placeholder-файл (например, `SanityTests.cs` с одним passing-тестом), чтобы убедиться, что тестовый раннер корректно обнаруживает и выполняет тесты.

### 11.3. Бизнес-тесты

Бизнес-тесты (валидация правил, расчёт стоимости, проверка переработки, optimistic concurrency и т. д.) отложены на этап реализации бизнес-логики. На данном этапе создаётся только инфраструктура для тестирования.

---

## 12. Учёт NOTES.md в архитектуре

Следующие допущения из `NOTES.md` учтены в архитектурных решениях и будут реализованы на этапе бизнес-логики:

| Допущение из NOTES.md | Архитектурное следствие |
|---|---|
| Календарные даты `yyyy-MM-dd` | Value object `DateOnly` (или обёртка над `DateOnly`) в Domain; сериализация через `System.Text.Json` с форматом `yyyy-MM-dd`. |
| Изменение ставки задним числом | Сущность `EmployeeRate` с историей (effective date); команда изменения ставки создаёт новую запись истории, не модифицируя существующие. |
| Динамический расчёт стоимости | Стоимость не хранится как персистентное поле, а вычисляется при чтении на основе ставки, действующей на дату записи. Query-обработчик будет использовать aggregation или in-memory расчёт. |
| Optimistic concurrency | Поле `_version` (или `__v`) в MongoDB-документе `TimeEntry`; команда обновления проверяет совпадение версии, при несовпадении возвращает ошибку 409. |
| REST-методы POST/PUT | Контроллеры на следующем этапе: `POST /api/time-entries` для создания, `PUT /api/time-entries/{id}` для изменения. |
| Ошибки `{ code, message }` | Global exception filter / middleware в Api; `ValidationException` из Application маппится в 400, `ConcurrencyException` в 409. |
| Переработка > 12 часов за день | Domain-сервис или query, суммирующий часы сотрудника за календарный день. |
| Закрытый период блокирует запись | Domain-правило, проверяющее наличие закрытого периода для даты записи. |

---

## 13. Отложенные решения (следующий этап)

Следующие вопросы сознательно не решаются на этапе каркаса и будут рассмотрены при реализации бизнес-логики:

1. **Бизнес-сущности и value objects.** Конкретный набор классов в Domain (Employee, Project, TimeEntry, EmployeeRate, PeriodClosure) и их структура.
2. **Индексы MongoDB.** Стратегия индексирования (compound indexes, text indexes) определяется после анализа query-паттернов.
3. **Aggregation pipeline.** Сложные запросы (отчёты, итоги по фильтрам, признак переработки) реализуются через aggregation framework — конкретные pipeline'ы определяются на следующем этапе.
4. **Seed данных.** Начальные данные (проекты, сотрудники) — формат и механизм загрузки.
5. **Mapster или ручной маппинг.** Выбор инструмента маппинга между domain-моделями и DTO/contract'ами.
6. **Global exception handling.** Конкретная реализация middleware / exception filter для возврата `{ code, message }`.
7. **Логирование и telemetry.** Структурное логирование (Serilog / OpenTelemetry) — выбор и конфигурация.
8. **Авторизация и аутентификация.** Не входит в текущий этап; при добавлении потребуется регистрация auth middleware и `UseAuthorization`.
9. **Health checks.** Проверки доступности MongoDB и других зависимостей.
10. **Docker и docker-compose.** Контейнеризация приложения и MongoDB для локальной разработки.

---

## 14. Критерии приемки

Этап считается завершённым, если выполнены все следующие условия:

1. Решение `Timesheet.sln` содержит 4 production-проекта и 4 тестовых проекта.
2. `dotnet restore` выполняется без ошибок для всего решения.
3. `dotnet build` выполняется без ошибок и предупреждений (с учётом `TreatWarningsAsErrors`).
4. `dotnet test` выполняется без ошибок; каждый тестовый проект содержит как минимум один passing-тест.
5. `Directory.Build.props` задаёт `net8.0`, `Nullable`, `ImplicitUsings`, `Deterministic` для всех проектов.
6. `Directory.Packages.props` содержит все версии пакетов; ни один `.csproj` не указывает версию пакета напрямую.
7. `Timesheet.Domain` не имеет project references на другие проекты решения.
8. `Timesheet.Application` ссылается только на `Timesheet.Domain`.
9. `Timesheet.Infrastructure` ссылается на `Timesheet.Application` и `Timesheet.Domain`.
10. `Timesheet.Api` ссылается на `Timesheet.Application` и `Timesheet.Infrastructure`.
11. Тестовые проекты ссылаются только на соответствующий тестируемый проект.
12. `MongoDB.Driver` используется только в `Timesheet.Infrastructure`; ни Domain, ни Application не содержат ссылок на MongoDB.
13. `IMongoClient` и `IMongoDatabase` зарегистрированы в DI как singleton.
14. MediatR зарегистрирован в Application с `ValidationBehavior`.
15. `Program.cs` не содержит `UseAuthorization`.
16. При старте приложения не выполняются ping MongoDB, миграции, создание индексов или seed.
17. Mapster отсутствует в решении.
18. Бизнес-сущности, endpoint'ы, commands/queries, репозитории отсутствуют.

### Команды проверки

```bash
# Из корня Backend/
dotnet restore Timesheet.sln
dotnet build Timesheet.sln --configuration Release --no-restore
dotnet test Timesheet.sln --configuration Release --no-build
```

---

## 15. Риски

| Риск | Вероятность | Влияние | Митигация |
|---|---|---|---|
| Несовместимость версий MediatR и FluentValidation | Низкая | Среднее | Фиксация версий в `Directory.Packages.props`; проверка совместимости при `restore`. |
| MongoDB.Driver v3.x содержит breaking changes относительно v2.x | Средняя | Среднее | Использование актуальной документации; при возникновении проблем — откат на стабильную v2.x. |
| Пустые проекты без кода могут вызвать путаницу у разработчиков | Низкая | Низкое | Данный документ + README с описанием этапа. |
| Отсутствие global exception handler на этом этапе — ошибки будут возвращать 500 | Высокое | Низкое | Приемлемо для каркаса; handler реализуется на следующем этапе вместе с бизнес-логикой. |
| `TreatWarningsAsErrors` может заблокировать сборку при обновлении NuGet-пакетов | Низкая | Низкое | Обновление пакетов выполняется контролируемо с проверкой сборки. |

---

## 16. Примечание об адаптации шаблона

Документ `Шаблон архитектуры.md`, присутствующий в корне репозитория, использован как источник идей и общих принципов (Clean Architecture, разделение на слои, CQRS). Однако структура, описанная в данном документе, является адаптацией, а не копией шаблона:

- Добавлен Vertical Slice как организационный принцип внутри Application.
- MongoDB.Driver используется напрямую, без ORM-обёрток, которые могут подразумеваться шаблоном.
- Mapster отложен до этапа реализации бизнес-логики.
- Тестовые проекты организованы в отдельной папке `tests/`, а не вложены в соответствующие production-проекты.
- Central Package Management через `Directory.Packages.props` — единая точка управления версиями.

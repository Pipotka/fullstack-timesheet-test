# План: структура backend Timesheet

> Дата: 2026-08-24
> Основа: `docs/superpowers/specs/2026-08-24-backend-structure-design.md`
> Цель этапа: каркас Clean Architecture, централизованные пакеты, базовая инфраструктура MediatR, FluentValidation и MongoDB без бизнес-логики

---

## 1. Контекст

Репозиторий содержит утверждённый дизайн структуры backend. В `Backend/` находится единственный проект `Timesheet.Api` (webapi-шаблон `dotnet new webapi`) с решением `Timesheet.sln`. Необходимо создать скелет решения из четырёх production-проектов и четырёх тестовых проектов, настроить централизованное управление пакетами, подключить MediatR с FluentValidation в слое Application и `MongoDB.Driver` в слое Infrastructure.

Все пути ниже указаны относительно корня репозитория (`Backend/` — корень решения).

---

## 2. Явные исключения (не делается на этом этапе)

Следующие компоненты сознательно отложены и не создаются ни в одном из шагов данного плана:

- Доменные сущности, value objects, доменные события (Employee, Project, TimeEntry, EmployeeRate, PeriodClosure и любые другие).
- Endpoint'ы контроллеров и маршруты.
- Commands и Queries (MediatR-обработчики, `IRequest<T>`, `IRequestHandler<TRequest, TResponse>`).
- Репозитории и интерфейсы репозиториев (порты).
- Seed данных.
- Индексы MongoDB.
- Бизнес-тесты (валидация правил, расчёт стоимости, переработка, optimistic concurrency).
- Mapster или любой другой маппер.
- Авторизация и аутентификация.
- Ping MongoDB, миграции, создание индексов и seed при старте приложения.
- Global exception handling middleware / filter.
- Логирование и telemetry (Serilog, OpenTelemetry).
- Docker и docker-compose.

---

## 3. Пошаговые задачи

### Шаг 1. Создать `Backend/Directory.Build.props`

**Файл:** `Backend/Directory.Build.props`

**Содержимое:**

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

**Ожидаемый результат:** файл создан; все проекты под `Backend/` наследуют `net8.0`, `Nullable`, `ImplicitUsings`, `Deterministic`, `TreatWarningsAsErrors` и включают Central Package Management.

---

### Шаг 2. Создать `Backend/Directory.Packages.props`

**Файл:** `Backend/Directory.Packages.props`

**Содержимое:**

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
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.11" />
  </ItemGroup>
</Project>
```

**Ожидаемый результат:** файл создан; все версии NuGet-пакетов зафиксированы в едином месте. Ни один `.csproj` не будет содержать атрибут `Version` у `PackageReference`.

---

### Шаг 3. Создать проект `Timesheet.Domain`

**Команды (из `Backend/`):**

```bash
dotnet new classlib --name Timesheet.Domain --output Timesheet.Domain --framework net8.0
```

**Действия после генерации:**

- Удалить файл `Timesheet.Domain/Class1.cs` (проект не содержит кода на этом этапе).
- Отредактировать `Timesheet.Domain/Timesheet.Domain.csproj`: удалить `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>` (наследуются из `Directory.Build.props`). Проект не содержит `<PackageReference>` и `<ProjectReference>`.

**Итоговый `Timesheet.Domain/Timesheet.Domain.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
</Project>
```

**Ожидаемый результат:** проект создан, не имеет исходящих зависимостей, не содержит кода.

---

### Шаг 4. Создать проект `Timesheet.Application`

**Команды (из `Backend/`):**

```bash
dotnet new classlib --name Timesheet.Application --output Timesheet.Application --framework net8.0
```

**Действия после генерации:**

- Удалить файл `Timesheet.Application/Class1.cs`.
- Отредактировать `Timesheet.Application/Timesheet.Application.csproj`: удалить `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`, добавить `PackageReference` без версий.

**Итоговый `Timesheet.Application/Timesheet.Application.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="MediatR" />
    <PackageReference Include="FluentValidation" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
  </ItemGroup>
</Project>
```

**Ожидаемый результат:** проект создан, содержит ссылки на MediatR и FluentValidation без указания версий (версии из CPM).

---

### Шаг 5. Создать проект `Timesheet.Infrastructure`

**Команды (из `Backend/`):**

```bash
dotnet new classlib --name Timesheet.Infrastructure --output Timesheet.Infrastructure --framework net8.0
```

**Действия после генерации:**

- Удалить файл `Timesheet.Infrastructure/Class1.cs`.
- Отредактировать `Timesheet.Infrastructure/Timesheet.Infrastructure.csproj`: удалить `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`, добавить `PackageReference` без версий.

**Итоговый `Timesheet.Infrastructure/Timesheet.Infrastructure.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="MongoDB.Driver" />
  </ItemGroup>
</Project>
```

**Ожидаемый результат:** проект создан, содержит ссылку на `MongoDB.Driver` без указания версии.

---

### Шаг 6. Очистить `Timesheet.Api/Timesheet.Api.csproj`

**Файл:** `Backend/Timesheet.Api/Timesheet.Api.csproj`

**Действия:** удалить `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`. У `PackageReference Include="Swashbuckle.AspNetCore"` удалить атрибут `Version="6.6.2"`. Project references на Application и Infrastructure будут добавлены на шаге 9.

**Итоговый `Timesheet.Api/Timesheet.Api.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" />
  </ItemGroup>
</Project>
```

**Ожидаемый результат:** csproj не содержит дублирующихся свойств и версий пакетов.

---

### Шаг 7. Добавить project references

**Команды (из `Backend/`):**

```bash
dotnet add Timesheet.Application/Timesheet.Application.csproj reference Timesheet.Domain/Timesheet.Domain.csproj
dotnet add Timesheet.Infrastructure/Timesheet.Infrastructure.csproj reference Timesheet.Application/Timesheet.Application.csproj
dotnet add Timesheet.Infrastructure/Timesheet.Infrastructure.csproj reference Timesheet.Domain/Timesheet.Domain.csproj
dotnet add Timesheet.Api/Timesheet.Api.csproj reference Timesheet.Application/Timesheet.Application.csproj
dotnet add Timesheet.Api/Timesheet.Api.csproj reference Timesheet.Infrastructure/Timesheet.Infrastructure.csproj
```

**Ожидаемая графа зависимостей production-проектов:**

```
Timesheet.Domain          <- ни от кого не зависит (ядро)
Timesheet.Application     -> Timesheet.Domain
Timesheet.Infrastructure  -> Timesheet.Application, Timesheet.Domain
Timesheet.Api             -> Timesheet.Application, Timesheet.Infrastructure
```

Api не ссылается на Domain напрямую (зависимость приходит транзитивно через Application).

---

### Шаг 8. Создать тестовые проекты

**Команды (из `Backend/`):**

```bash
dotnet new xunit --name Timesheet.Domain.Tests --output tests/Timesheet.Domain.Tests --framework net8.0
dotnet new xunit --name Timesheet.Application.Tests --output tests/Timesheet.Application.Tests --framework net8.0
dotnet new xunit --name Timesheet.Infrastructure.Tests --output tests/Timesheet.Infrastructure.Tests --framework net8.0
dotnet new xunit --name Timesheet.Api.Tests --output tests/Timesheet.Api.Tests --framework net8.0
```

Шаблон `dotnet new xunit` автоматически генерирует файл `UnitTest1.cs` с одним passing-тестом. Этот файл используется как минимальная тестовая заготовка (шаг 14).

**Действия после генерации для каждого тестового проекта:**

- Отредактировать `.csproj`: удалить `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`.
- Убедиться, что все `PackageReference` не содержат атрибута `Version` (версии из CPM).

**Итоговые `.csproj` тестовых проектов:**

`tests/Timesheet.Domain.Tests/Timesheet.Domain.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
```

`tests/Timesheet.Application.Tests/Timesheet.Application.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="NSubstitute" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
```

`tests/Timesheet.Infrastructure.Tests/Timesheet.Infrastructure.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="NSubstitute" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
```

`tests/Timesheet.Api.Tests/Timesheet.Api.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
```

**Ожидаемый результат:** четыре тестовых проекта созданы в `Backend/tests/`, каждый содержит `UnitTest1.cs` с passing-тестом, пакеты без версий.

---

### Шаг 9. Добавить project references в тестовые проекты

**Команды (из `Backend/`):**

```bash
dotnet add tests/Timesheet.Domain.Tests/Timesheet.Domain.Tests.csproj reference Timesheet.Domain/Timesheet.Domain.csproj
dotnet add tests/Timesheet.Application.Tests/Timesheet.Application.Tests.csproj reference Timesheet.Application/Timesheet.Application.csproj
dotnet add tests/Timesheet.Infrastructure.Tests/Timesheet.Infrastructure.Tests.csproj reference Timesheet.Infrastructure/Timesheet.Infrastructure.csproj
dotnet add tests/Timesheet.Api.Tests/Timesheet.Api.Tests.csproj reference Timesheet.Api/Timesheet.Api.csproj
```

**Ожидаемая графа зависимостей тестовых проектов:**

```
Timesheet.Domain.Tests         -> Timesheet.Domain
Timesheet.Application.Tests    -> Timesheet.Application
Timesheet.Infrastructure.Tests -> Timesheet.Infrastructure
Timesheet.Api.Tests            -> Timesheet.Api
```

Кросс-ссылки между тестовыми проектами запрещены.

---

### Шаг 10. Создать `Timesheet.Application/DependencyInjection.cs`

**Файл:** `Backend/Timesheet.Application/DependencyInjection.cs`

**Содержимое:**

```csharp
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Timesheet.Application.Common.Behaviors;

namespace Timesheet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}
```

**Ожидаемый результат:** метод `AddApplication` регистрирует MediatR (сканирование assembly Application) и `ValidationBehavior` как open-generic transient.

---

### Шаг 11. Создать `Timesheet.Application/Common/Behaviors/ValidationBehavior.cs`

**Файл:** `Backend/Timesheet.Application/Common/Behaviors/ValidationBehavior.cs`

**Содержимое:**

```csharp
using FluentValidation;
using MediatR;

namespace Timesheet.Application.Common.Behaviors;

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

**Ожидаемый результат:** pipeline-поведение выполняет все зарегистрированные валидаторы для запроса MediatR; при наличии ошибок выбрасывает `ValidationException` из FluentValidation.

---

### Шаг 12. Создать `Timesheet.Infrastructure/DependencyInjection.cs`

**Файл:** `Backend/Timesheet.Infrastructure/DependencyInjection.cs`

**Содержимое:**

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Timesheet.Infrastructure;

public static class DependencyInjection
{
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

        return services;
    }
}
```

**Ожидаемый результат:** `IMongoClient` и `IMongoDatabase` зарегистрированы как singleton. Сетевые операции (ping, создание индексов, миграции, seed) при старте не выполняются. `MongoClient` создаётся лениво через factory-делегат.

---

### Шаг 13. Создать `Timesheet.Infrastructure/MongoDb/MongoDbSettings.cs`

**Файл:** `Backend/Timesheet.Infrastructure/MongoDb/MongoDbSettings.cs`

**Содержимое:**

```csharp
namespace Timesheet.Infrastructure.MongoDb;

public sealed record MongoDbSettings
{
    public string ConnectionString { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = string.Empty;
}
```

**Ожидаемый результат:** record связан с секцией `MongoDb` в `appsettings.json` через `IOptions`-паттерн (bind через `GetSection().Get<T>()`).

---

### Шаг 14. Обновить `Backend/Timesheet.Api/Program.cs`

**Файл:** `Backend/Timesheet.Api/Program.cs`

**Итоговое содержимое:**

```csharp
using Timesheet.Application;
using Timesheet.Infrastructure;

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

**Изменения относительно текущего файла:**

- Удалена строка `app.UseAuthorization()`.
- Добавлены `using Timesheet.Application;` и `using Timesheet.Infrastructure;`.
- Добавлены вызовы `builder.Services.AddApplication()` и `builder.Services.AddInfrastructure(builder.Configuration)`.

**Ожидаемый результат:** `Program.cs` не содержит `UseAuthorization`; DI-контейнер включает регистрации из Application (MediatR, ValidationBehavior) и Infrastructure (MongoDB).

---

### Шаг 15. Обновить `Backend/Timesheet.Api/appsettings.json`

**Файл:** `Backend/Timesheet.Api/appsettings.json`

**Итоговое содержимое:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "Timesheet"
  }
}
```

**Ожидаемый результат:** секция `MongoDb` доступна для `IConfiguration` при вызове `AddInfrastructure`.

---

### Шаг 16. Создать минимальные тестовые заготовки

Шаблон `dotnet new xunit` (шаг 8) уже создал файл `UnitTest1.cs` в каждом тестовом проекте с одним passing-тестом. Переименовать файлы для ясности:

- `tests/Timesheet.Domain.Tests/UnitTest1.cs` -> `tests/Timesheet.Domain.Tests/SanityTests.cs`
- `tests/Timesheet.Application.Tests/UnitTest1.cs` -> `tests/Timesheet.Application.Tests/SanityTests.cs`
- `tests/Timesheet.Infrastructure.Tests/UnitTest1.cs` -> `tests/Timesheet.Infrastructure.Tests/SanityTests.cs`
- `tests/Timesheet.Api.Tests/UnitTest1.cs` -> `tests/Timesheet.Api.Tests/SanityTests.cs`

В каждом файле обновить имя класса на `SanityTests` и заменить содержимое. Шаблон `SanityTests.cs` (единый для всех четырёх проектов, отличается только namespace):

```csharp
using FluentAssertions;

namespace Timesheet.<Layer>.Tests;

public class SanityTests
{
    [Fact]
    public void Infrastructure_Is_Ready()
    {
        true.Should().BeTrue();
    }
}
```

Где `<Layer>` — Domain, Application, Infrastructure или Api соответственно.

**Ожидаемый результат:** каждый тестовый проект содержит один passing-тест, подтверждающий работоспособность тестового раннера.

---

### Шаг 17. Обновить `Backend/Timesheet.sln`

**Команды (из `Backend/`):**

```bash
dotnet sln Timesheet.sln add Timesheet.Domain/Timesheet.Domain.csproj
dotnet sln Timesheet.sln add Timesheet.Application/Timesheet.Application.csproj
dotnet sln Timesheet.sln add Timesheet.Infrastructure/Timesheet.Infrastructure.csproj
dotnet sln Timesheet.sln add tests/Timesheet.Domain.Tests/Timesheet.Domain.Tests.csproj
dotnet sln Timesheet.sln add tests/Timesheet.Application.Tests/Timesheet.Application.Tests.csproj
dotnet sln Timesheet.sln add tests/Timesheet.Infrastructure.Tests/Timesheet.Infrastructure.Tests.csproj
dotnet sln Timesheet.sln add tests/Timesheet.Api.Tests/Timesheet.Api.Tests.csproj
```

Проект `Timesheet.Api` уже присутствует в решении и не требует повторного добавления.

**Ожидаемый результат:** `Timesheet.sln` содержит 8 проектов (4 production + 4 тестовых).

---

### Шаг 18. Выполнить restore

**Команда (из `Backend/`):**

```bash
dotnet restore Timesheet.sln
```

**Ожидаемый результат:** все пакеты загружены без ошибок. Ни один `PackageReference` не вызывает предупреждения NU1604 (missing version) благодаря `Directory.Packages.props`.

---

### Шаг 19. Выполнить build

**Команда (из `Backend/`):**

```bash
dotnet build Timesheet.sln --configuration Release --no-restore
```

**Ожидаемый результат:** сборка завершается без ошибок и без предупреждений (`TreatWarningsAsErrors` активен). Все 8 проектов компилируются успешно.

---

### Шаг 20. Выполнить test

**Команда (из `Backend/`):**

```bash
dotnet test Timesheet.sln --configuration Release --no-build
```

**Ожидаемый результат:** все 4 тестовых проекта проходят; каждый выполняет минимум 1 passing-тест из `SanityTests.cs`. Итого не менее 4 passing-тестов.

---

## 4. Статические проверки

После успешного `restore` / `build` / `test` выполняются проверки, подтверждающие соответствие архитектурным ограничениям.

### Проверка 1. Отсутствие ORM

**Команда (из `Backend/`):**

```bash
grep -r "MongoDB.EntityFrameworkCore\|MongoRepository\|MongoDbGenericRepository" --include="*.csproj" .
```

**Ожидаемый результат:** вывод пуст. Ни один `.csproj` не содержит ссылок на ORM-обёртки.

### Проверка 2. Отсутствие авторизации

**Команда (из `Backend/`):**

```bash
grep -r "UseAuthorization\|UseAuthentication\|AddAuthorization\|AddAuthentication" --include="*.cs" .
```

**Ожидаемый результат:** вывод пуст. `Program.cs` не содержит вызовов auth middleware.

### Проверка 3. Отсутствие подключения MongoDB при старте

**Команда (из `Backend/`):**

```bash
grep -rn "Ping\|RunMongoMigration\|CreateIndexes\|SeedData\|EnsureIndexes" --include="*.cs" Timesheet.Infrastructure/ Timesheet.Api/
```

**Ожидаемый результат:** вывод пуст. Infrastructure регистрирует только `IMongoClient` и `IMongoDatabase` через DI-фабрики без сетевых вызовов.

### Проверка 4. MongoDB.Driver только в Infrastructure

**Команда (из `Backend/`):**

```bash
grep -r "MongoDB.Driver" --include="*.csproj" Timesheet.Domain/ Timesheet.Application/ Timesheet.Api/
```

**Ожидаемый результат:** вывод пуст. `MongoDB.Driver` присутствует только в `Timesheet.Infrastructure/Timesheet.Infrastructure.csproj`.

### Проверка 5. Отсутствие Mapster

**Команда (из `Backend/`):**

```bash
grep -r "Mapster" --include="*.csproj" --include="*.cs" .
```

**Ожидаемый результат:** вывод пуст. Mapster не подключён ни в одном проекте.

### Проверка 6. Отсутствие бизнес-артефактов

**Команда (из `Backend/`):**

```bash
grep -rn "class Employee\|class Project\|class TimeEntry\|class EmployeeRate\|class PeriodClosure\|IRequest<\|IRequestHandler<\|IRepository\|IMongoRepository" --include="*.cs" Timesheet.Domain/ Timesheet.Application/ Timesheet.Infrastructure/ Timesheet.Api/
```

**Ожидаемый результат:** вывод пуст. Доменные сущности, MediatR-обработчики, интерфейсы репозиториев не определены.

### Проверка 7. CPM: отсутствие версий в csproj

**Команда (из `Backend/`):**

```bash
grep -rn 'PackageReference.*Version=' --include="*.csproj" .
```

**Ожидаемый результат:** вывод пуст. Все версии пакетов управляются централизованно через `Directory.Packages.props`.

### Проверка 8. Domain не имеет исходящих project references

**Команда (из `Backend/`):**

```bash
grep "ProjectReference" Timesheet.Domain/Timesheet.Domain.csproj
```

**Ожидаемый результат:** вывод пуст. Domain — независимое ядро.

---

## 5. Коммит

**Команды (из корня репозитория):**

```bash
git add docs/superpowers/plans/2026-08-24-backend-structure.md
git status
git commit -m "Добавить план структуры backend" -m "Зафиксирован план создания слоёв Clean Architecture, централизованных пакетов и базовой инфраструктуры MediatR, FluentValidation и MongoDB."
```

**Ожидаемый результат:**

- В `git status` (staged) присутствует только файл `docs/superpowers/plans/2026-08-24-backend-structure.md`.
- Файлы `LICENSE`, `NOTES.md`, `code-review/*`, `Backend/*`, `Шаблон архитектуры.md`, `test-task.html`, `test-task.pdf` не добавлены в индекс.
- Коммит создан с указанным сообщением и описанием.

---

## 6. Итоговая структура файлов

После выполнения всех шагов дерево `Backend/` будет иметь вид:

```
Backend/
├── Directory.Build.props                          (новый)
├── Directory.Packages.props                       (новый)
├── Timesheet.sln                                  (обновлён: +7 проектов)
├── .gitignore                                     (без изменений)
│
├── Timesheet.Domain/
│   └── Timesheet.Domain.csproj                    (обновлён: убраны дубли свойств)
│
├── Timesheet.Application/
│   ├── Timesheet.Application.csproj               (обновлён: +PackageReference, +ProjectReference)
│   ├── DependencyInjection.cs                     (новый)
│   └── Common/
│       └── Behaviors/
│           └── ValidationBehavior.cs              (новый)
│
├── Timesheet.Infrastructure/
│   ├── Timesheet.Infrastructure.csproj             (обновлён: +PackageReference, +ProjectReference)
│   ├── DependencyInjection.cs                     (новый)
│   └── MongoDb/
│       └── MongoDbSettings.cs                     (новый)
│
├── Timesheet.Api/
│   ├── Timesheet.Api.csproj                       (обновлён: убраны дубли свойств, убрана версия)
│   ├── Program.cs                                 (обновлён: -UseAuthorization, +AddApplication, +AddInfrastructure)
│   ├── appsettings.json                           (обновлён: +секция MongoDb)
│   ├── appsettings.Development.json               (без изменений)
│   └── Properties/
│       └── launchSettings.json                    (без изменений)
│
└── tests/
    ├── Timesheet.Domain.Tests/
    │   ├── Timesheet.Domain.Tests.csproj           (новый)
    │   └── SanityTests.cs                         (новый)
    ├── Timesheet.Application.Tests/
    │   ├── Timesheet.Application.Tests.csproj      (новый)
    │   └── SanityTests.cs                         (новый)
    ├── Timesheet.Infrastructure.Tests/
    │   ├── Timesheet.Infrastructure.Tests.csproj   (новый)
    │   └── SanityTests.cs                         (новый)
    └── Timesheet.Api.Tests/
        ├── Timesheet.Api.Tests.csproj              (новый)
        └── SanityTests.cs                         (новый)
```

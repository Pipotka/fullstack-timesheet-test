# REVIEW

## TimesheetReportHandler.cs

### 1. Список проблем
#### 1. Неверный выбор ставки: берётся первая ставка из списка, а не ставка, действовавшая на дату записи

**Файл:** `TimesheetReportHandler.cs`

**Фрагмент:**

```csharp
var rate = employee.Rates.FirstOrDefault().Value;
```

**Суть проблемы:**

По бизнес-правилу стоимость записи должна рассчитываться как:

```text
часы × ставка сотрудника, действовавшая на дату записи
```

Текущий код просто берёт первую ставку из списка:

```csharp
employee.Rates.FirstOrDefault()
```

Дата записи при этом не учитывается.

Это неверно, потому что:

- ставки могут быть не отсортированы;
- первой может оказаться будущая ставка;
- первой может оказаться старая ставка;
- для одной и той же записи может примениться не та ставка, которая действовала на дату.

**Чем грозит в продакшене:**

Отчёт будет считать неверные суммы.

Например, из приёмочных данных:

| Дата       | Сотрудник | Часы | Ожидаемая ставка | Ожидаемая стоимость |
| ---------- | --------- | ---: | ---------------: | ------------------: |
| 20.02.2026 | Иванов    |    8 |              500 |               4 000 |
| 05.03.2026 | Иванов    |    8 |              600 |               4 800 |

Если код возьмёт первую попавшуюся ставку, отчёт за февраль или март может оказаться неверным.

**Как чинить:**

Нужно выбирать ставку, которая действовала на дату записи:

```csharp
var rate = employee.Rates
    .Where(r => r.From.Date <= entry.Date.Date)
    .OrderByDescending(r => r.From)
    .Select(r => r.Value)
    .FirstOrDefault();
```

По бизнес правлу нельзя создать запись табеля, если на момент записи у сотрудника нет действующей ставки.

---

#### 2. Деньги считаются в `double`

**Файл:** `TimesheetReportHandler.cs`

**Фрагменты:**

```csharp
public double Hours { get; set; }
public double Amount { get; set; }
public double Budget { get; set; }
public double Percent { get; set; }
```

```csharp
var amount = Math.Round(entry.Hours * rate, 2);
```

**Суть проблемы:**

В тестовом задании явно указано:

> Деньги — decimal, округление до копеек. `double` и `float` для денег не использовать.

В коде деньги представлены типом `double`.

**Чем грозит в продакшене:**

`double` использует двоичную плавающую точку и плохо подходит для финансовых расчётов. Возможны:

- ошибки округления;
- нестабильные суммы;
- расхождения между расчётами на сервере и ожидаемыми бухгалтерскими данными;
- проблемы при сравнении значений.

**Как чинить:**

Использовать `decimal` для:

- ставок;
- бюджетов;
- стоимости;
- суммарных стоимостей;
- возможно, часов, так как они участвуют в расчёте денег.

---

#### 3. Загрузка всех TimeEntry в память

**Файл:** `TimesheetReportHandler.cs`

**Фрагмент:**

```csharp
var entries = await _db.GetCollection<TimeEntry>("time_entries")
    .Find(FilterDefinition<TimeEntry>.Empty)
    .ToListAsync();

var monthEntries = entries
    .Where(e => e.Date.Year == request.Year && e.Date.Month == request.Month)
    .ToList();
```

**Суть проблемы:**

Код загружает в память все записи из коллекции `time_entries`, а затем уже в C# фильтрует их по месяцу.

**Чем грозит в продакшене:**

- полный перебор коллекции;
- долгая работа отчёта;
- высокая нагрузка на MongoDB;
- загрузка большого объёма данных в память приложения;
- риск нехватки памяти;
- давление на сборщик мусора;
- невозможность нормально работать на больших данных.

**Как чинить:**

применить фильтрацию сразу на стороне MongoDB.

Также важно фильтровать месяц не через:

```csharp
e.Date.Year == request.Year && e.Date.Month == request.Month
```

а через диапазон дат:

```csharp
Date >= monthStart && Date < nextMonthStart
```

Это позволяет эффективнее использовать индексы.

---

#### 4. Запросы к сотрудникам и проектам выполняются в цикле, часть из них блокируется через `.Result`

**Файл:** `TimesheetReportHandler.cs`

**Фрагменты:**

```csharp
foreach (var entry in monthEntries)
{
    var employee = _db.GetCollection<Employee>("employees")
        .Find(e => e.Id == entry.EmployeeId)
        .FirstOrDefaultAsync().Result;
```

```csharp
if (!rows.ContainsKey(entry.ProjectId))
{
    var project = await _db.GetCollection<Project>("projects")
        .Find(p => p.Id == entry.ProjectId)
        .FirstOrDefaultAsync();
```

**Суть проблемы:**

Здесь есть несколько связанных проблем:

1. Сотрудники запрашиваются отдельно для каждой записи.
2. Проекты запрашиваются отдельно для каждого нового проекта.
3. В запросе сотрудника используется `.Result`.
4. Метод асинхронный, но часть кода блокирует поток.

Это приводит к проблеме `N+1 query`.

**Чем грозит в продакшене:**

Если за месяц есть 10 000 записей, код может сделать тысячи запросов к коллекции `employees`, даже если самих сотрудников всего 50.

Последствия:

- лишняя нагрузка на MongoDB;
- медленный отчёт;
- риск таймаутов;
- неэффективное использование потоков;
- `.Result` может привести к блокировке потока.

**Как чинить:**

Минимально:

- заранее получить список нужных сотрудников одним запросом;
- заранее получить список нужных проектов одним запросом;
- заменить `.Result` на `await`;
- передавать `CancellationToken` в асинхронные вызовы.

Примерно:

```csharp
var employeeIds = monthEntries
    .Select(e => e.EmployeeId)
    .Distinct()
    .ToList();

var employees = await _db.GetCollection<Employee>("employees")
    .Find(e => employeeIds.Contains(e.Id))
    .ToListAsync(token);

var employeesById = employees.ToDictionary(e => e.Id);
```

Аналогично для проектов.

Но для миллионов записей лучше использовать агрегацию на стороне MongoDB.

---

#### 5. Модель отчёта не соответствует требованиям: нет признака риска и итоговой строки

**Файл:** `TimesheetReportHandler.cs`

**Фрагменты:**

```csharp
public class ProjectReportRow
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public double Hours { get; set; }
    public double Amount { get; set; }
    public double Budget { get; set; }
    public double Percent { get; set; }
    public bool Overspent { get; set; }
}
```

```csharp
public class GetProjectReportQuery : IRequest<List<ProjectReportRow>>
```

**Суть проблемы:**

По требованиям отчёт по проектам за месяц должен возвращать:

- часы;
- стоимость;
- бюджет;
- процент освоения бюджета;
- признак перерасхода, больше 100%;
- признак риска, больше 80%;
- итоговую строку.

В текущем коде есть только признак перерасхода:

```csharp
public bool Overspent { get; set; }
```

Признака риска нет.

Также обработчик возвращает только список строк:

```csharp
Task<List<ProjectReportRow>>
```

Итоговой строки нет.

**Чем грозит в продакшене:**

Отчёт не соответствует заявленным требованиям. Фронтенду придётся отдельно досчитывать итоги и высчитывать признак риска.

**Как чинить:**

Добавить поле риска:

```csharp
public bool Risk { get; set; }
```

Вернуть не просто список строк, а полноценный ответ отчёта, например:

```csharp
public class ProjectReportResponse
{
    public List<ProjectReportRow> Rows { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
}
```

---

#### 6. Вычисление процента не учитывает нулевой бюджет

**Файл:** `TimesheetReportHandler.cs`

**Фрагмент:**

```csharp
row.Percent = Math.Round(row.Amount / row.Budget * 100, 2);
```

**Суть проблемы:**

Если бюджет проекта равен нулю, вычисление процента становится некорректным.

Для `double` это может привести к:

```text
Infinity
NaN
```

После перевода денег на `decimal` такое деление может привести к исключению.

**Чем грозит в продакшене:**

- некорректный процент освоения бюджета;
- неправильные флаги перерасхода и риска;
- возможное падение отчёта;
- некорректное отображение данных пользователю.

**Как чинить:**

Явно обрабатывать случай `Budget == 0`.

Например:

- если `Amount == 0`, считать процент как `0`;
- если `Amount > 0`, считать проект перерасходованным/рисковым;
- либо возвращать отдельную пометку, что бюджет не задан.

Конкретное поведение зависит от бизнес правил.

---

#### 7. Нет защитной обработки отсутствующих сотрудников или проектов

**Файл:** `TimesheetReportHandler.cs`

**Фрагменты:**

```csharp
var employee = _db.GetCollection<Employee>("employees")
    .Find(e => e.Id == entry.EmployeeId)
    .FirstOrDefaultAsync().Result;

var rate = employee.Rates.FirstOrDefault().Value;
```

```csharp
var project = await _db.GetCollection<Project>("projects")
    .Find(p => p.Id == entry.ProjectId)
    .FirstOrDefaultAsync();

rows[entry.ProjectId] = new ProjectReportRow
{
    ProjectId = project.Id,
    ProjectName = project.Name,
    Budget = project.Budget
};
```

**Суть проблемы:**

Если сотрудник или проект не найдены, код упадёт с ошибкой, например:

```text
NullReferenceException
```

Сотрудника могут удалить из базы и тогда запрос может вернуть `null`.

**Чем грозит в продакшене:**

Вместо понятного ответа пользователь может получить `500 Internal Server Error`.

**Как чинить:**

Проверять результаты запросов:

```csharp
if (employee == null)
{
    // логировать и обработать
}
```

```csharp
if (project == null)
{
    // логировать и обработать
}
```

Варианты обработки:

- пропустить запись;
- вернуть бизнес-ошибку.

Конкретное поведение зависит от бизнес правил.

---

#### 8. `CancellationToken` принят, но не используется

**Файл:** `TimesheetReportHandler.cs`

**Фрагмент:**

```csharp
public async Task<List<ProjectReportRow>> Handle(
    GetProjectReportQuery request,
    CancellationToken token)
```

Но ниже токен не передаётся в вызовы к БД:

```csharp
.ToListAsync();
.FirstOrDefaultAsync();
```

**Суть проблемы:**

Обработчик получает `CancellationToken`, но не использует его.

**Чем грозит в продакшене:**

Если построение отчёта будет долгим, его нельзя будет корректно отменить, например, если пользователь закрыл страницу или клиент разорвал соединение.

**Как чинить:**

Передавать токен в асинхронные операции:

```csharp
.ToListAsync(token)
.FirstOrDefaultAsync(token)
```

---

#### 9. Косметические замечания

##### 9.1. Отсутствие XML-комментариев

Для данного тестового задания это не является важной проблемой, так как код и так понятен но в идеале нужно добавить кмментарии.

##### 9.2. Использовать record для Dto

Использование неизменяех записей позволит избежать случайного изменения исходных данных запроса и помимо этого record можно компактно описать.

---

### 2. Что бы вы изменили в структуре этого кода

Да, ты прав: в `2.1` лучше передавать не отдельные `year` и `month`, а объект запроса/DTO. В `2.2` нужно переформулировать из режима «я бы советовал» в готовый пункт ревью.

Ниже — исправленный вариант, который можно класть в `REVIEW.md`.

Я выбрал вариант с отдельным DTO `ProjectReportRequest`, потому что так сервис чтения не зависит напрямую от MediatR-контракта. Но при желании его можно заменить на `GetProjectReportQuery` — MediatR это не запрещает.

---

#### 2.1. Вынести построение отчёта из обработчика в отдельный сервис чтения

Сейчас `TimesheetReportHandler` делает слишком много:

- запрашивает данные из базы;
- фильтрует записи по месяцу;
- считает стоимость;
- группирует данные по проектам;
- вычисляет процент освоения бюджета;
- формирует итоговую коллекцию.

Построение отчёта стоит вынести в отдельный сервис чтения, например:

```csharp
public sealed record ProjectReportRequest(int Year, int Month);

public interface IProjectReportReader
{
    Task<ProjectReportResponse> GetProjectReportAsync(
        ProjectReportRequest request,
        CancellationToken token);
}
```

Тогда обработчик станет тоньше и будет отвечать только за маршрутизацию запроса:

```csharp
public class TimesheetReportHandler
    : IRequestHandler<GetProjectReportQuery, ProjectReportResponse>
{
    private readonly IProjectReportReader _reportReader;

    public TimesheetReportHandler(IProjectReportReader reportReader)
    {
        _reportReader = reportReader;
    }

    public Task<ProjectReportResponse> Handle(
        GetProjectReportQuery request,
        CancellationToken token)
    {
        var reportRequest = new ProjectReportRequest(request.Year, request.Month);

        return _reportReader.GetProjectReportAsync(reportRequest, token);
    }
}
```
Это улучшит читаемость и упростит тестирование.

---

#### 2.2. Выделить сценарные сервисы чтения

Прямую работу с `IMongoDatabase` внутри обработчика стоит отдельными сервисами под конкретные сценарии чтения.

Например:

```csharp
public interface IProjectReportReader
{
    Task<ProjectReportResponse> GetProjectReportAsync(
        ProjectReportRequest request,
        CancellationToken token);
}

public interface ITimeEntriesReader
{
    Task<PagedResult<TimeEntryDto>> GetTimeEntriesAsync(
        TimeEntriesQuery query,
        CancellationToken token);
}

public interface IEmployeeDirectoryReader
{
    Task<List<EmployeeDto>> GetEmployeesAsync(CancellationToken token);
}

public interface IProjectDirectoryReader
{
    Task<List<ProjectDto>> GetProjectsAsync(CancellationToken token);
}
```
В результате запросы к MongoDB и бизнес-правила чтения будут сосредоточены в отдельных сервисах, а обработчики останутся тонкими и будут только вызывать нужный сценарий.

---

#### 2.3. Вынести правило выбора ставки по дате в доменный сервис

Выбор ставки по дате - это важное бизнес-правило. Оно нужно не только в отчёте, но и при создании/изменении записи табеля.

Я бы вынес его отдельно:

```csharp
public interface IEmployeeRateCalculator
{
    decimal GetEffectiveRate(Employee employee, DateTime date);
}
```

Или хотя бы сделать отдельный метод:

```csharp
private static decimal GetEffectiveRate(Employee employee, DateTime entryDate)
{
    var rate = employee.Rates
        .Where(r => r.From.Date <= entryDate.Date)
        .OrderByDescending(r => r.From)
        .Select(r => r.Value)
        .FirstOrDefault();

    if (rate == null)
    {
        throw new BusinessException(
            "NO_RATE_ON_DATE",
            $"На дату {entryDate:dd.MM.yyyy} у сотрудника нет действующей ставки.");
    }

    return rate.Value;
}
```

Плюсы:

- правило можно переиспользовать;
- его легко покрыть юнит-тестами;
- оно не будет дублироваться в разных местах.

---

#### 2.4. Ввести единый подход к деньгам

Вместо `double` везде, где есть деньги, использовать `decimal`.

Например:

```csharp
public class Rate
{
    public DateTime From { get; set; }
    public decimal Value { get; set; }
}
```

```csharp
public class Project
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Budget { get; set; }
}
```

```csharp
public class ProjectReportRow
{
    public decimal Hours { get; set; }
    public decimal Amount { get; set; }
    public decimal Budget { get; set; }
    public decimal Percent { get; set; }
}
```

---

#### 2.5. Сделать модель ответа отчёта полноценной

Сейчас отчёт возвращает:

```csharp
List<ProjectReportRow>
```

По требованиям нужна итоговая строка.

Я бы сделал так:

```csharp
public class ProjectReportResponse
{
    public List<ProjectReportRow> Rows { get; set; } = new();
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
}
```

А строка отчёта:

```csharp
public class ProjectReportRow
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public decimal Hours { get; set; }
    public decimal Amount { get; set; }
    public decimal Budget { get; set; }
    public decimal Percent { get; set; }
    public bool Overspent { get; set; }
    public bool Risk { get; set; }
}
```

---


## TimeEntriesPage.tsx

### 1. Список проблем

---

#### 1. `useEffect` без массива зависимостей приводит к бесконечным запросам

**Файл:** `TimeEntriesPage.tsx`

**Фрагмент:**

```tsx
useEffect(() => {
    load();
});
```

**Суть проблемы:**

У `useEffect` не указан массив зависимостей. Это значит, что эффект будет выполняться после каждого рендера.

Внутри `load()` происходит:

```tsx
setEntries(data);
```

Обновление состояния вызывает новый рендер, после которого снова выполняется `useEffect`, снова вызывается `load()`, снова приходит ответ, снова вызывается `setEntries`.

**Чем грозит в продакшене:**

Фактически это бесконечный цикл запросов к серверу:

- постоянные запросы к `/api/time-entries`;
- лишняя нагрузка на backend;
- деградация интерфейса;
- возможный перегрев сети/сервера;
- пользователь может столкнуться с лагами и подвисанием страницы.

**Как чинить:**

Указать зависимости, при которых нужно перезагружать список:

```tsx
useEffect(() => {
    load();
}, [props.year, props.month]);
```

---

#### 2. Сохранение считается успешным даже при ошибке сервера

**Файл:** `TimeEntriesPage.tsx`

**Фрагмент:**

```tsx
const save = async () => {
    const body = {
        employeeId: employeeId,
        projectId: projectId,
        date: new Date(date).toLocaleDateString(),
        hours: hours,
    };

    await fetch("/api/time-entries", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
    });

    entries.push(body);
    setEntries(entries);
    alert("Сохранено");
};
```

**Суть проблемы:**

Код не проверяет:

```tsx
response.ok
```

То есть даже если сервер вернул:

- `400` — ошибка валидации;
- `409` — конфликт;
- `422` — бизнес-ошибка;
- `500` — внутренняя ошибка;

фронтенд всё равно выполнит:

```tsx
entries.push(body);
setEntries(entries);
alert("Сохранено");
```

**Чем грозит в продакшене:**

Пользователь будет думать, что запись сохранена, хотя на самом деле сервер её не создал.

**Как чинить:**

Проверять статус ответа и показывать ошибку пользователю.

После успешного сохранения лучше перезагрузить список с сервера.

---

#### 3. После сохранения запись добавляется в список напрямую, минуя сервер

**Файл:** `TimeEntriesPage.tsx`

**Фрагмент:**

```tsx
entries.push(body);
setEntries(entries);
```

**Суть проблемы:**

Здесь сразу несколько проблем.

##### 3.1. Мутируется текущий массив состояния

В React нельзя мутировать состояние напрямую:

```tsx
entries.push(body);
```

Нужно создавать новый массив:

```tsx
setEntries([...entries, newEntry]);
```

##### 3.2. `setEntries(entries)` передаёт ту же ссылку

После мутации кода:

```tsx
entries.push(body);
setEntries(entries);
```

в `setEntries` передаётся тот же самый массив. React может не понять, что состояние изменилось, и не перерисовать список корректно.

##### 3.3. В `body` нет данных, которые приходят с сервера

Локально созданный объект содержит только:

```tsx
{
    employeeId,
    projectId,
    date,
    hours
}
```

Но таблица рендерит:

```tsx
entry.date
entry.employeeName
entry.projectName
entry.hours
entry.amount
entry.id
```

У новой записи не будет:

- `id`;
- `employeeName`;
- `projectName`;
- `amount`;
- ставки;
- флага переработки;
- других серверных полей.

**Чем грозит в продакшене:**

После «сохранения»:

- строка может отображаться пустыми значениями;
- `entry.amount.toFixed(2)` может упасть;
- кнопка «Удалить» может отправить запрос вида:

```http
DELETE /api/time-entries/undefined
```

**Как чинить:**

Не добавлять запись в список вручную.

Вариант исправления:

```tsx
await load();
```

То есть после успешного сохранения заново загрузить актуальные данные с сервера.

---

#### 4. Нет обработки ошибок при загрузке и удалении

**Файл:** `TimeEntriesPage.tsx`

**Фрагменты:**

```tsx
const load = async () => {
    setLoading(true);
    const response = await fetch("/api/time-entries?year=" + props.year + "&month=" + props.month);
    const data = await response.json();
    setEntries(data);
    setLoading(false);
};
```

```tsx
const remove = async (id: string) => {
    await fetch("/api/time-entries/" + id, { method: "DELETE" });
    load();
};
```

**Суть проблемы:**

Код не проверяет:

- успешно ли завершился запрос;
- вернулся ли корректный ответ;
- был ли серверный код ошибки.

Также нет `try / catch / finally`.

**Чем грозит в продакшене:**

Если запрос завершится ошибкой:

- `loading` может остаться навсегда;
- пользователь не поймёт, что загрузка не удалась;
- удаление может молча не сработать;
- интерфейс будет показывать неактуальные данные.

**Как чинить:**

Добавить обработку ошибок и проверку `response.ok`.


---

#### 5. Использование `any[]` и небезопасное обращение к полям

**Файл:** `TimeEntriesPage.tsx`

**Фрагменты:**

```tsx
const [entries, setEntries] = useState<any[]>([]);
const [employees, setEmployees] = useState<any[]>([]);
```

```tsx
<td>{entry.amount.toFixed(2)}</td>
```

```tsx
total = total + parseFloat(filtered[i].amount);
```

**Суть проблемы:**

Тип `any` отключает основную пользу TypeScript.

Компонент не знает, какие поля есть у записи:

- `id`;
- `date`;
- `employeeId`;
- `employeeName`;
- `projectId`;
- `projectName`;
- `hours`;
- `amount`;
- `comment`;
- `rate`;
- `overtime`.

**Чем грозит в продакшене:**

Если поле будет называться иначе, отсутствовать или придёт `null`, интерфейс может упасти.

Например:

```tsx
entry.amount.toFixed(2)
```

```tsx
parseFloat(filtered[i].amount)
```

**Как чинить:**

Ввести нормальные типы.

Пример:

```tsx
interface TimeEntryDto {
    id: string;
    date: string;
    employeeId: string;
    employeeName: string;
    projectId: string;
    projectName: string;
    hours: number;
    rate: number;
    amount: number;
    comment?: string;
    overtime?: boolean;
}

interface EmployeeDto {
    id: string;
    name: string;
}
```

И состояние:

```tsx
const [entries, setEntries] = useState<TimeEntryDto[]>([]);
const [employees, setEmployees] = useState<EmployeeDto[]>([]);
```

---

#### 6. Дата отправляется в локальном формате

**Файл:** `TimeEntriesPage.tsx`

**Фрагмент:**

```tsx
date: new Date(date).toLocaleDateString(),
```

**Суть проблемы:**

`toLocaleDateString()` возвращает дату в формате, зависящем от локали пользователя.

Например:

```text
20.02.2026
2/20/2026
2026-02-20
```

Это неоднозначный формат для передачи на сервер.

**Чем грозит в продакшене:**

Сервер может неправильно распознать дату.

Например, в некоторых форматах:

```text
01.02.2026
```

может означать:

- 1 февраля;
- 2 января.

**Как чинить:**

Использовать предсказуемый формат, например ISO или использовать:

```tsx
<input type="date" />
```

и хранить значение в формате:

```text
yyyy-MM-dd
```

---

#### 7. Форма добавления использует состояние фильтра сотрудника

**Файл:** `TimeEntriesPage.tsx`

**Фрагменты:**

```tsx
const [employeeId, setEmployeeId] = useState("");
```

```tsx
const filtered = employeeId
    ? entries.filter((e) => e.employeeId == employeeId)
    : entries;
```

```tsx
const body = {
    employeeId: employeeId,
    projectId: projectId,
    date: new Date(date).toLocaleDateString(),
    hours: hours,
};
```

**Суть проблемы:**

Переменная `employeeId` используется одновременно для двух разных вещей:

1. фильтр списка записей;
2. сотрудник в форме добавления новой записи.

Это приводит к путанице.

Если пользователь выбрал:

```text
Все сотрудники
```

то `employeeId === ""`.

При сохранении будет отправлено:

```json
{
    "employeeId": ""
}
```

Если пользователь выбрал сотрудника, чтобы отфильтровать список, этот же сотрудник автоматически подставится в форму добавления.

**Чем грозит в продакшене:**

- неочевидный пользовательский сценарий;
- можно случайно создать запись не на того сотрудника;
- при пустом фильтре можно отправить некорректный `employeeId`.

**Как чинить:**

Разделить состояние:

```tsx
const [filterEmployeeId, setFilterEmployeeId] = useState("");
const [formEmployeeId, setFormEmployeeId] = useState("");
```

И использовать их по назначению.

---

#### 8. Нет валидации формы на клиенте

**Файл:** `TimeEntriesPage.tsx`

**Фрагмент:**

```tsx
const save = async () => {
    const body = {
        employeeId: employeeId,
        projectId: projectId,
        date: new Date(date).toLocaleDateString(),
        hours: hours,
    };
```

**Суть проблемы:**

Перед отправкой не проверяется:

- выбран ли сотрудник;
- выбран ли проект;
- введена ли дата;
- введены ли часы;
- часы положительные;
- часы кратны `0.5`;
- часы не больше `24`;
- дата корректная.

**Чем грозит в продакшене:**

Можно отправить:

```json
{
    "employeeId": "",
    "projectId": "",
    "date": "Invalid Date",
    "hours": ""
}
```

Даже если сервер всё это валидирует можно пресекать часть запросов к api, которые наверняка вернуться с ошибкой валидации.

**Как чинить:**

Добавить клиентскую валидацию.

---

#### 9. Поле проекта вводится вручную вместо выбора из справочника

**Файл:** `TimeEntriesPage.tsx`

**Фрагмент:**

```tsx
<input
    placeholder="Проект"
    value={projectId}
    onChange={(e) => setProjectId(e.target.value)}
/>
```

**Суть проблемы:**

В тестовом задании есть отдельный эндпоинт:

```text
GET /api/projects
```

Он предназначен для справочников и выпадающих списков.

Но в коде проект вводится вручную как текст.

**Чем грозит в продакшене:**

Пользователь может ввести:

- несуществующий идентификатор;
- опечатку;
- пустое значение;
- название проекта вместо идентификатора.

**Как чинить:**

Загружать проекты и использовать `<select>`.

---

#### 10. Экран не соответствует части требований из тестового задания

Это не один конкретный баг, но важный пункт, если сверять код с постановкой.

**Файл:** `TimeEntriesPage.tsx`

**Суть проблемы:**

По тестовому заданию экран «Табель» должен иметь:

- фильтры: месяц, сотрудник, проект;
- таблицу с полями:
  - дата;
  - сотрудник;
  - проект;
  - часы;
  - ставка;
  - стоимость;
  - комментарий;
  - отметка переработки;
- добавление, редактирование и удаление через модальное окно;
- ошибки валидации с сервера в интерфейсе;
- итоги по отфильтрованному списку:
  - часы;
  - стоимость;
- реальную пагинацию на стороне БД.

В текущем коде:

- нет фильтра по проекту;
- нет редактирования записи;
- нет модального окна;
- нет отображения ставки;
- нет комментария;
- нет отметки переработки;
- нет пагинации;
- итоги считают только стоимость, но не часы;
- серверные ошибки валидации не отображаются пользователю.

**Чем грозит в продакшене:**

Функциональность экрана не соответствует требованиям продукта.

**Как чинить:**

Добавить недостающие элементы:

- фильтр проекта;
- пагинацию;
- модальную форму;
- редактирование;
- отображение ставки, комментария и переработки;
- итоговые часы;
- отображение ошибок сервера.

---

#### 11. Использование `==` вместо `===`

**Файл:** `TimeEntriesPage.tsx`

**Фрагмент:**

```tsx
const filtered = employeeId
    ? entries.filter((e) => e.employeeId == employeeId)
    : entries;
```

**Суть проблемы:**

Оператор `==` выполняет приведение типов.

**Чем грозит в продакшене:**

Возможны неочевидные баги из-за скрытого приведения типов.

**Как чинить:**
Использовать `===`

---

#### 12. Использование `alert`

**Файл:** `TimeEntriesPage.tsx`

**Фрагмент:**

```tsx
alert("Сохранено");
```

**Суть проблемы:**

`alert`:

- блокирует интерфейс;
- плохо выглядит;
- не позволяет показать разные типы сообщений;
- не соответствует нормальному пользовательскому опыту.

**Чем грозит в продакшене:**

Плохой пользовательский опыт.

**Как чинить:**

Использовать:

- сообщение рядом с формой;
- тосты;
- компонент уведомлений;
- ошибки в форме.

---

#### 13. Косметические и второстепенные замечания

##### 13.1. Кнопка добавления не блокируется во время запроса

Можно случайно нажать несколько раз и отправить несколько запросов.

##### 13.2. Инлайновые стили

Это сделано для уменьшение количества файлов, но в реальном проекте лучше вынести стили или использовать компонентную библиотеку.

---

### 2. Что можно изменить в структуре `TimeEntriesPage.tsx`

---

#### 2.1. Вынести работу с сервером в отдельный API-клиент

Сейчас `fetch` вызывается прямо внутри компонента:

```tsx
fetch("/api/time-entries?year=" + props.year + "&month=" + props.month);
```

```tsx
fetch("/api/time-entries/" + id, { method: "DELETE" });
```

Лучше вынести это в отдельный модуль.

Это даст:

- единое место для обработки ошибок;
- типизированные запросы и ответы;
- более чистый компонент;
- возможность легко заменить `fetch` на другую библиотеку.

---

#### 2.2. Ввести типы запросов и ответов

Вместо:

```tsx
any[]
```

лучше использовать модели.

Пример:

```ts
interface TimeEntryDto {
    id: string;
    date: string;
    employeeId: string;
    employeeName: string;
    projectId: string;
    projectName: string;
    hours: number;
    rate: number;
    amount: number;
    comment?: string;
    overtime?: boolean;
}

interface EmployeeDto {
    id: string;
    name: string;
}

interface ProjectDto {
    id: string;
    code: string;
    name: string;
}
```

Форма:

```ts
interface TimeEntryFormValues {
    employeeId: string;
    projectId: string;
    date: string;
    hours: string;
}
```

Это значительно повысит надёжность кода на TypeScript.

---

#### 2.3. Разделить состояние списка, фильтров и формы

Сейчас в одном компоненте смешаны:

- загрузка списка;
- фильтрация;
- форма добавления;
- итоги;
- ошибки;
- состояния загрузки.

Лучше разделить состояние:

```tsx
const [filters, setFilters] = useState<TimeEntriesFilters>({
    year: props.year,
    month: props.month,
    employeeId: "",
    projectId: "",
    page: 1,
    pageSize: 50,
});

const [form, setForm] = useState<TimeEntryFormValues>({
    employeeId: "",
    projectId: "",
    date: "",
    hours: "",
});
```

Это сделает код понятнее и позволит нормально добавить фильтр по проекту, пагинацию и редактирование.

---

#### 2.4. Вынести форму добавления/редактирования в отдельный компонент

Сейчас форма находится прямо в общем экране.

Лучше сделать отдельный компонент.

---

#### 2.5. Вынести таблицу в отдельный компонент

Сейчас таблица рендерится прямо в экране.

Если появятся:

- пагинация;
- сортировка;
- редактирование;
- подсветка переработки;
- итоги;
- колонки со ставкой и комментариями;

компонент станет слишком большим.

---
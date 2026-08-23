// TimesheetReportHandler.fixed.cs
//
// Исправлены:
// 1. Выбор действующей ставки на дату записи.
// 2. Фильтрация месяца на стороне БД: Date >= monthStart && Date < nextMonthStart.
// 3. Пакетная загрузка сотрудников и проектов.
// 4. Использование await вместо .Result.
//
// Осознанно не исправлено в этом минимальном фиксе:
// - деньги всё ещё считаются в double;
// - отчёт всё ещё не является полноценной агрегацией в MongoDB;
// - нет итоговой строки, признака риска и полноценной обработки бизнес-ошибок.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;

namespace Demo.Api.Queries.Reports
{
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

    public class GetProjectReportQuery : IRequest<List<ProjectReportRow>>
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class TimesheetReportHandler : IRequestHandler<GetProjectReportQuery, List<ProjectReportRow>>
    {
        private readonly IMongoDatabase _db;

        public TimesheetReportHandler(IMongoDatabase db)
        {
            _db = db;
        }

        public async Task<List<ProjectReportRow>> Handle(
            GetProjectReportQuery request,
            CancellationToken token)
        {
            var monthStart = new DateTime(request.Year, request.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            // FIXED: фильтруем месяц на стороне БД через диапазон дат,
            // а не выгружаем все записи в память.
            var entries = await _db.GetCollection<TimeEntry>("time_entries")
                .Find(e => e.Date >= monthStart && e.Date < nextMonthStart)
                .ToListAsync(token);

            if (entries.Count == 0)
            {
                return new List<ProjectReportRow>();
            }

            var employeeIds = entries
                .Select(e => e.EmployeeId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var projectIds = entries
                .Select(e => e.ProjectId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            // FIXED: сотрудники загружаются пакетно, одним запросом.
            var employees = new List<Employee>();

            if (employeeIds.Count > 0)
            {
                employees = await _db.GetCollection<Employee>("employees")
                    .Find(e => employeeIds.Contains(e.Id))
                    .ToListAsync(token);
            }

            // FIXED: проекты загружаются пакетно, одним запросом.
            var projects = new List<Project>();

            if (projectIds.Count > 0)
            {
                projects = await _db.GetCollection<Project>("projects")
                    .Find(p => projectIds.Contains(p.Id))
                    .ToListAsync(token);
            }

            var employeesById = employees
                .Where(e => !string.IsNullOrWhiteSpace(e.Id))
                .ToDictionary(e => e.Id);

            var projectsById = projects
                .Where(p => !string.IsNullOrWhiteSpace(p.Id))
                .ToDictionary(p => p.Id);

            var rows = new Dictionary<string, ProjectReportRow>();

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.EmployeeId) || string.IsNullOrWhiteSpace(entry.ProjectId))
                {
                    continue;
                }

                if (!employeesById.TryGetValue(entry.EmployeeId, out var employee))
                {
                    // В идеальных данных такой ситуации быть не должно.
                    // Если нужно считать это ошибкой, здесь можно бросать бизнес-исключение.
                    continue;
                }

                // FIXED: выбираем последнюю ставку, которая начала действовать не позднее даты записи.
                var effectiveRate = employee.Rates?
                    .Where(r => r.From.Date <= entry.Date.Date)
                    .OrderByDescending(r => r.From)
                    .FirstOrDefault();

                if (effectiveRate == null)
                {
                    // Если на дату записи нет действующей ставки, запись не должна была существовать.
                    // В идеальных данных такой ситуации нет. При необходимости можно бросать бизнес-ошибку.
                    continue;
                }

                var amount = Math.Round(entry.Hours * effectiveRate.Value, 2);

                if (!rows.TryGetValue(entry.ProjectId, out var row))
                {
                    if (!projectsById.TryGetValue(entry.ProjectId, out var project))
                    {
                        // В идеальных данных такой ситуации быть не должно.
                        // Если нужно считать это ошибкой, здесь можно бросать бизнес-исключение.
                        continue;
                    }

                    row = new ProjectReportRow
                    {
                        ProjectId = project.Id,
                        ProjectName = project.Name,
                        Budget = project.Budget
                    };

                    rows[entry.ProjectId] = row;
                }

                row.Hours += entry.Hours;
                row.Amount += amount;
            }

            foreach (var row in rows.Values)
            {
                // Минимальная защита от нулевого бюджета.
                if (row.Budget == 0)
                {
                    row.Percent = 0;
                    row.Overspent = row.Amount > 0;
                }
                else
                {
                    row.Percent = Math.Round(row.Amount / row.Budget * 100, 2);
                    row.Overspent = row.Percent > 100;
                }
            }

            return rows.Values
                .OrderBy(r => r.ProjectName)
                .ToList();
        }
    }

    // --- сущности (упрощённо) ---
    public class TimeEntry
    {
        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string ProjectId { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public string Comment { get; set; }
    }

    public class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Rate> Rates { get; set; }
    }

    public class Rate
    {
        public DateTime From { get; set; }
        public double Value { get; set; }
    }

    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Budget { get; set; }
    }
}
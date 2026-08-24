import { useMemo } from 'react';
import type { Employee } from '../../entities/employee/types';
import type { Project } from '../../entities/project/types';
import { useRepositories } from '../../app/providers/useRepositories';
import { getMonthName, getCurrentYearMonth } from '../../shared/lib/date-utils';

interface TimesheetFiltersProps {
  year: number;
  month: number;
  employeeId: string;
  projectId: string;
  onFilterChange: (filters: { year?: number; month?: number; employeeId?: string; projectId?: string }) => void;
  onAdd: () => void;
}

const ALL_YEARS = [2025, 2026, 2027];

export function TimesheetFilters({
  year,
  month,
  employeeId,
  projectId,
  onFilterChange,
  onAdd,
}: TimesheetFiltersProps) {
  const repos = useRepositories();
  const employees: Employee[] = repos.employees.getAll();
  const projects: Project[] = repos.projects.getAll();

  const currentYear = getCurrentYearMonth().year;
  const years = useMemo(() => ALL_YEARS.filter((y) => y <= currentYear), [currentYear]);

  const months = useMemo(
    () => Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: getMonthName(i + 1) })),
    [],
  );

  return (
    <div className="d-flex flex-wrap align-items-end gap-3 mb-3">
      <div>
        <label className="form-label mb-1 small text-muted">Месяц</label>
        <div className="d-flex gap-2">
          <select
            className="form-select form-select-sm"
            value={month}
            onChange={(e) => onFilterChange({ month: Number(e.target.value) })}
          >
            {months.map((m) => (
              <option key={m.value} value={m.value}>
                {m.label}
              </option>
            ))}
          </select>
          <select
            className="form-select form-select-sm"
            style={{ width: 'auto' }}
            value={year}
            onChange={(e) => onFilterChange({ year: Number(e.target.value) })}
          >
            {years.map((y) => (
              <option key={y} value={y}>
                {y}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div>
        <label className="form-label mb-1 small text-muted">Сотрудник</label>
        <select
          className="form-select form-select-sm"
          value={employeeId}
          onChange={(e) => onFilterChange({ employeeId: e.target.value })}
        >
          <option value="">Все сотрудники</option>
          {employees.map((emp) => (
            <option key={emp.id} value={emp.id}>
              {emp.name}
            </option>
          ))}
        </select>
      </div>

      <div>
        <label className="form-label mb-1 small text-muted">Проект</label>
        <select
          className="form-select form-select-sm"
          value={projectId}
          onChange={(e) => onFilterChange({ projectId: e.target.value })}
        >
          <option value="">Все проекты</option>
          {projects.map((p) => (
            <option key={p.id} value={p.id}>
              {p.code} — {p.name}
            </option>
          ))}
        </select>
      </div>

      <button className="btn btn-primary btn-sm ms-auto" onClick={onAdd}>
        + Добавить запись
      </button>
    </div>
  );
}

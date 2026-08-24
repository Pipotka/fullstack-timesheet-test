import type { Employee } from '../../../entities/employee/types';
import type { Project } from '../../../entities/project/types';
import type {
  TimeEntry,
  TimeEntryDto,
  TimeEntryUpdateDto,
  TimeEntryView,
} from '../../../entities/time-entry/types';
import type { ClosedPeriod } from '../../../entities/closed-period/types';
import type { ProjectReport } from '../../../entities/report/types';
import type { DateString } from '../../../entities/rate/types';
import type { PaginatedResult } from '../../types/pagination';
import type { ApiError } from '../../types/api-error';
import type {
  EmployeeRepository,
  ProjectRepository,
  TimeEntryRepository,
  ClosedPeriodRepository,
  ReportRepository,
  Repositories,
  TimeEntryFilters,
  TimeEntryTotals,
} from '../contracts';
import { roundMoney } from '../../lib/money-utils';
import { isDateInRange, getMonthBounds } from '../../lib/date-utils';
import {
  seedEmployees,
  seedProjects,
  seedTimeEntries,
  seedClosedPeriods,
} from './seed-data';

// ─── Shared mutable state ────────────────────────────────────────────────────

interface DataStore {
  employees: Employee[];
  projects: Project[];
  timeEntries: TimeEntry[];
  closedPeriods: ClosedPeriod[];
  nextId: number;
}

function createStore(): DataStore {
  return {
    employees: structuredClone(seedEmployees),
    projects: structuredClone(seedProjects),
    timeEntries: structuredClone(seedTimeEntries),
    closedPeriods: structuredClone(seedClosedPeriods),
    nextId: 100,
  };
}

let store = createStore();

// ─── Helpers ─────────────────────────────────────────────────────────────────

function getRateForDate(employee: Employee, date: DateString): number | null {
  const applicable = employee.rates
    .filter((r) => r.from <= date)
    .sort((a, b) => (a.from < b.from ? 1 : -1));
  return applicable.length > 0 ? applicable[0].value : null;
}

function getEmployeeDayHours(employeeId: string, date: DateString, excludeId?: string): number {
  return store.timeEntries
    .filter((e) => e.employeeId === employeeId && e.date === date && e.id !== excludeId)
    .reduce((sum, e) => sum + e.hours, 0);
}

function isOvertimeDay(employeeId: string, date: DateString): boolean {
  const total = store.timeEntries
    .filter((e) => e.employeeId === employeeId && e.date === date)
    .reduce((sum, e) => sum + e.hours, 0);
  return total > 12;
}

function toView(entry: TimeEntry): TimeEntryView {
  const employee = store.employees.find((e) => e.id === entry.employeeId);
  const project = store.projects.find((p) => p.id === entry.projectId);
  const rate = employee ? (getRateForDate(employee, entry.date) ?? 0) : 0;
  return {
    id: entry.id,
    employeeName: employee?.name ?? '—',
    projectCode: project?.code ?? '—',
    date: entry.date,
    hours: entry.hours,
    rate,
    amount: roundMoney(entry.hours * rate),
    comment: entry.comment,
    isOvertime: isOvertimeDay(entry.employeeId, entry.date),
    version: entry.version,
  };
}

function makeError(code: string, message: string): ApiError {
  return { code, message };
}

// ─── Employee Repository ─────────────────────────────────────────────────────

class MockEmployeeRepository implements EmployeeRepository {
  getAll(): Employee[] {
    return store.employees;
  }
  getById(id: string): Employee | undefined {
    return store.employees.find((e) => e.id === id);
  }
}

// ─── Project Repository ──────────────────────────────────────────────────────

class MockProjectRepository implements ProjectRepository {
  getAll(): Project[] {
    return store.projects;
  }
  getById(id: string): Project | undefined {
    return store.projects.find((p) => p.id === id);
  }
}

// ─── Closed Period Repository ────────────────────────────────────────────────

class MockClosedPeriodRepository implements ClosedPeriodRepository {
  isClosed(year: number, month: number): boolean {
    return store.closedPeriods.some((p) => p.year === year && p.month === month);
  }
  close(year: number, month: number): void {
    if (!this.isClosed(year, month)) {
      store.closedPeriods.push({ year, month });
    }
  }
  open(year: number, month: number): void {
    store.closedPeriods = store.closedPeriods.filter(
      (p) => !(p.year === year && p.month === month),
    );
  }
}

// ─── Time Entry Repository ───────────────────────────────────────────────────

class MockTimeEntryRepository implements TimeEntryRepository {
  list(
    filters: TimeEntryFilters,
    page: number,
    pageSize: number,
  ): PaginatedResult<TimeEntryView> {
    const { from, to } = getMonthBounds(filters.year, filters.month);
    let filtered = store.timeEntries.filter(
      (e) => e.date >= from && e.date <= to,
    );
    if (filters.employeeId) {
      filtered = filtered.filter((e) => e.employeeId === filters.employeeId);
    }
    if (filters.projectId) {
      filtered = filtered.filter((e) => e.projectId === filters.projectId);
    }
    filtered.sort((a, b) => (a.date < b.date ? -1 : a.date > b.date ? 1 : 0));

    const totalCount = filtered.length;
    const start = (page - 1) * pageSize;
    const pageItems = filtered.slice(start, start + pageSize);

    return {
      items: pageItems.map(toView),
      totalCount,
      page,
      pageSize,
    };
  }

  getTotals(filters: TimeEntryFilters): TimeEntryTotals {
    const { from, to } = getMonthBounds(filters.year, filters.month);
    let filtered = store.timeEntries.filter(
      (e) => e.date >= from && e.date <= to,
    );
    if (filters.employeeId) {
      filtered = filtered.filter((e) => e.employeeId === filters.employeeId);
    }
    if (filters.projectId) {
      filtered = filtered.filter((e) => e.projectId === filters.projectId);
    }

    let hours = 0;
    let amount = 0;
    for (const entry of filtered) {
      const employee = store.employees.find((e) => e.id === entry.employeeId);
      const rate = employee ? (getRateForDate(employee, entry.date) ?? 0) : 0;
      hours += entry.hours;
      amount += roundMoney(entry.hours * rate);
    }
    return { hours, amount: roundMoney(amount) };
  }

  create(dto: TimeEntryDto): { ok: true; entry: TimeEntryView } | { ok: false; error: ApiError } {
    // Validate hours
    if (dto.hours <= 0 || dto.hours > 24 || (dto.hours * 10) % 5 !== 0) {
      return {
        ok: false,
        error: makeError('INVALID_HOURS', 'Часы должны быть положительными, кратными 0.5 и не более 24'),
      };
    }

    // Validate employee
    const employee = store.employees.find((e) => e.id === dto.employeeId);
    if (!employee) {
      return { ok: false, error: makeError('EMPLOYEE_NOT_FOUND', 'Сотрудник не найден') };
    }

    // Validate project
    const project = store.projects.find((p) => p.id === dto.projectId);
    if (!project) {
      return { ok: false, error: makeError('PROJECT_NOT_FOUND', 'Проект не найден') };
    }

    // Validate project date range
    if (!isDateInRange(dto.date, project.startDate, project.endDate)) {
      return {
        ok: false,
        error: makeError(
          'DATE_OUT_OF_PROJECT',
          'Дата записи выходит за границы периода проекта',
        ),
      };
    }

    // Validate rate exists
    const rate = getRateForDate(employee, dto.date);
    if (rate === null) {
      return {
        ok: false,
        error: makeError(
          'NO_RATE',
          `У сотрудника ${employee.name} нет действующей ставки на дату ${dto.date}`,
        ),
      };
    }

    // Validate closed period
    const [y, m] = dto.date.split('-').map(Number);
    if (store.closedPeriods.some((p) => p.year === y && p.month === m)) {
      return {
        ok: false,
        error: makeError('PERIOD_CLOSED', 'Период закрыт для редактирования'),
      };
    }

    // Validate daily hours limit
    const dayHours = getEmployeeDayHours(dto.employeeId, dto.date);
    if (dayHours + dto.hours > 24) {
      return {
        ok: false,
        error: makeError(
          'DAILY_LIMIT',
          `Превышен лимит 24 часа за день: суммарно ${dayHours + dto.hours} ч.`,
        ),
      };
    }

    const entry: TimeEntry = {
      id: `te-${++store.nextId}`,
      employeeId: dto.employeeId,
      projectId: dto.projectId,
      date: dto.date,
      hours: dto.hours,
      comment: dto.comment,
      version: 1,
    };
    store.timeEntries.push(entry);
    return { ok: true, entry: toView(entry) };
  }

  update(
    id: string,
    dto: TimeEntryUpdateDto,
  ): { ok: true; entry: TimeEntryView } | { ok: false; error: ApiError } {
    const idx = store.timeEntries.findIndex((e) => e.id === id);
    if (idx === -1) {
      return { ok: false, error: makeError('NOT_FOUND', 'Запись не найдена') };
    }

    const existing = store.timeEntries[idx];

    // Version conflict
    if (existing.version !== dto.version) {
      return {
        ok: false,
        error: makeError(
          'VERSION_CONFLICT',
          'Запись была изменена. Перезагрузите данные и повторите.',
        ),
      };
    }

    // Validate hours
    if (dto.hours <= 0 || dto.hours > 24 || (dto.hours * 10) % 5 !== 0) {
      return {
        ok: false,
        error: makeError('INVALID_HOURS', 'Часы должны быть положительными, кратными 0.5 и не более 24'),
      };
    }

    // Validate employee
    const employee = store.employees.find((e) => e.id === dto.employeeId);
    if (!employee) {
      return { ok: false, error: makeError('EMPLOYEE_NOT_FOUND', 'Сотрудник не найден') };
    }

    // Validate project
    const project = store.projects.find((p) => p.id === dto.projectId);
    if (!project) {
      return { ok: false, error: makeError('PROJECT_NOT_FOUND', 'Проект не найден') };
    }

    // Validate project date range
    if (!isDateInRange(dto.date, project.startDate, project.endDate)) {
      return {
        ok: false,
        error: makeError(
          'DATE_OUT_OF_PROJECT',
          'Дата записи выходит за границы периода проекта',
        ),
      };
    }

    // Validate rate exists
    const rate = getRateForDate(employee, dto.date);
    if (rate === null) {
      return {
        ok: false,
        error: makeError(
          'NO_RATE',
          `У сотрудника ${employee.name} нет действующей ставки на дату ${dto.date}`,
        ),
      };
    }

    // Validate closed period
    const [y, m] = dto.date.split('-').map(Number);
    if (store.closedPeriods.some((p) => p.year === y && p.month === m)) {
      return {
        ok: false,
        error: makeError('PERIOD_CLOSED', 'Период закрыт для редактирования'),
      };
    }

    // Validate daily hours limit (exclude current entry)
    const dayHours = getEmployeeDayHours(dto.employeeId, dto.date, id);
    if (dayHours + dto.hours > 24) {
      return {
        ok: false,
        error: makeError(
          'DAILY_LIMIT',
          `Превышен лимит 24 часа за день: суммарно ${dayHours + dto.hours} ч.`,
        ),
      };
    }

    const updated: TimeEntry = {
      ...existing,
      employeeId: dto.employeeId,
      projectId: dto.projectId,
      date: dto.date,
      hours: dto.hours,
      comment: dto.comment,
      version: existing.version + 1,
    };
    store.timeEntries[idx] = updated;
    return { ok: true, entry: toView(updated) };
  }

  delete(id: string): { ok: true } | { ok: false; error: ApiError } {
    const idx = store.timeEntries.findIndex((e) => e.id === id);
    if (idx === -1) {
      return { ok: false, error: makeError('NOT_FOUND', 'Запись не найдена') };
    }

    const entry = store.timeEntries[idx];
    const [y, m] = entry.date.split('-').map(Number);
    if (store.closedPeriods.some((p) => p.year === y && p.month === m)) {
      return {
        ok: false,
        error: makeError('PERIOD_CLOSED', 'Период закрыт для редактирования'),
      };
    }

    store.timeEntries.splice(idx, 1);
    return { ok: true };
  }
}

// ─── Report Repository ───────────────────────────────────────────────────────

class MockReportRepository implements ReportRepository {
  getProjectReport(year: number, month: number): ProjectReport {
    const { from, to } = getMonthBounds(year, month);
    const entries = store.timeEntries.filter((e) => e.date >= from && e.date <= to);

    const map = new Map<
      string,
      { projectId: string; projectCode: string; projectName: string; hours: number; amount: number; budget: number }
    >();

    for (const entry of entries) {
      const project = store.projects.find((p) => p.id === entry.projectId);
      if (!project) continue;
      const employee = store.employees.find((e) => e.id === entry.employeeId);
      if (!employee) continue;
      const rate = getRateForDate(employee, entry.date) ?? 0;
      const amount = roundMoney(entry.hours * rate);

      const key = project.id;
      const existing = map.get(key);
      if (existing) {
        existing.hours += entry.hours;
        existing.amount = roundMoney(existing.amount + amount);
      } else {
        map.set(key, {
          projectId: project.id,
          projectCode: project.code,
          projectName: project.name,
          hours: entry.hours,
          amount,
          budget: project.budget,
        });
      }
    }

    let totalHours = 0;
    let totalAmount = 0;

    const rows = Array.from(map.values()).map((r) => {
      totalHours += r.hours;
      totalAmount = roundMoney(totalAmount + r.amount);

      const percent = r.budget > 0 ? roundMoney((r.amount / r.budget) * 100) : 0;
      return {
        projectId: r.projectId,
        projectCode: r.projectCode,
        projectName: r.projectName,
        hours: r.hours,
        amount: r.amount,
        budget: r.budget,
        percent,
        isRisk: percent > 80,
        isOverspent: percent > 100,
      };
    });

    rows.sort((a, b) => (a.projectCode < b.projectCode ? -1 : 1));

    return {
      rows,
      totals: { hours: totalHours, amount: totalAmount },
    };
  }
}

// ─── Factory ─────────────────────────────────────────────────────────────────

export function createRepositories(): Repositories {
  return {
    employees: new MockEmployeeRepository(),
    projects: new MockProjectRepository(),
    timeEntries: new MockTimeEntryRepository(),
    closedPeriods: new MockClosedPeriodRepository(),
    reports: new MockReportRepository(),
  };
}

// ─── Service: change rate (no UI, for scenario 8) ────────────────────────────

export function changeEmployeeRate(
  employeeId: string,
  fromDate: DateString,
  newValue: number,
): void {
  const emp = store.employees.find((e) => e.id === employeeId);
  if (!emp) return;
  const rate = emp.rates.find((r) => r.from === fromDate);
  if (rate) {
    rate.value = newValue;
  }
}

/** Reset store to seed data (useful for tests) */
export function resetStore(): void {
  store = createStore();
}

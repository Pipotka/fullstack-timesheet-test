import { useState, useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import type { TimeEntryView } from '../../entities/time-entry/types';
import type { TimeEntryFilters, TimeEntryTotals } from '../../shared/api/contracts';
import type { ApiError } from '../../shared/types/api-error';
import { useRepositories } from '../../app/providers/useRepositories';
import { getCurrentYearMonth } from '../../shared/lib/date-utils';
import {
  parseTimesheetQuery,
  serializeTimesheetQuery,
  replaceKnownParams,
  validateKnownId,
  TIMESHEET_QUERY_KEYS,
  type FilterDefaults,
  type TimesheetQuery,
} from '../../shared/lib/filter-query';

export interface UseTimesheetDataResult {
  entries: TimeEntryView[];
  totalCount: number;
  totals: TimeEntryTotals;
  loading: boolean;
  error: ApiError | null;
  page: number;
  pageSize: number;
  filters: TimeEntryFilters;
  setPage: (page: number) => void;
  setFilters: (filters: Partial<TimeEntryFilters>) => void;
  refresh: () => void;
}

export function useTimesheetData(): UseTimesheetDataResult {
  const repos = useRepositories();
  const [searchParams, setSearchParams] = useSearchParams();

  // Defaults: текущие год/месяц (стабильны в течение сессии)
  const defaults = useMemo<FilterDefaults>(() => {
    const { year, month } = getCurrentYearMonth();
    return { year, month };
  }, []);

  // Известные ID сотрудников и проектов для валидации
  const employees = repos.employees.getAll();
  const projects = repos.projects.getAll();
  const knownEmployeeIds = useMemo(() => new Set(employees.map((e) => e.id)), [employees]);
  const knownProjectIds = useMemo(() => new Set(projects.map((p) => p.id)), [projects]);

  // Парсим URL на каждый рендер — источник истины для состояния
  const query: TimesheetQuery = useMemo(
    () => parseTimesheetQuery(searchParams, defaults, knownEmployeeIds, knownProjectIds),
    [searchParams, defaults, knownEmployeeIds, knownProjectIds],
  );

  const pageSize = 10;
  const [error] = useState<ApiError | null>(null);
  // Счётчик ревизий для принудительного пересчёта после мутаций (CRUD)
  const [, setRevision] = useState(0);

  const filters = useMemo<TimeEntryFilters>(
    () => ({
      year: query.year,
      month: query.month,
      employeeId: query.employeeId || undefined,
      projectId: query.projectId || undefined,
    }),
    [query.year, query.month, query.employeeId, query.projectId],
  );

  // Синхронный расчёт данных (mock-данные синхронны)
  const result = repos.timeEntries.list(filters, query.page, pageSize);
  const totals = repos.timeEntries.getTotals(filters);

  /** Изменение фильтров → запись в URL, сброс page=1 */
  const setFiltersPartial = useCallback(
    (partial: Partial<TimeEntryFilters>) => {
      setSearchParams((prev) => {
        const current = parseTimesheetQuery(prev, defaults, knownEmployeeIds, knownProjectIds);
        const next: TimesheetQuery = {
          year: partial.year ?? current.year,
          month: partial.month ?? current.month,
          employeeId:
            partial.employeeId !== undefined
              ? validateKnownId(partial.employeeId, knownEmployeeIds)
              : current.employeeId,
          projectId:
            partial.projectId !== undefined
              ? validateKnownId(partial.projectId, knownProjectIds)
              : current.projectId,
          page: 1, // сброс страницы при изменении любого фильтра
        };
        const serialized = serializeTimesheetQuery(next, defaults);
        return replaceKnownParams(prev, TIMESHEET_QUERY_KEYS, serialized);
      });
    },
    [setSearchParams, defaults, knownEmployeeIds, knownProjectIds],
  );

  /** Изменение страницы → запись только page в URL, без очистки остальных query */
  const setPage = useCallback(
    (newPage: number) => {
      const totalPages = Math.max(1, Math.ceil(result.totalCount / pageSize));
      const clamped = Math.max(1, Math.min(newPage, totalPages));

      setSearchParams((prev) => {
        const current = parseTimesheetQuery(prev, defaults, knownEmployeeIds, knownProjectIds);
        const next: TimesheetQuery = { ...current, page: clamped };
        const serialized = serializeTimesheetQuery(next, defaults);
        return replaceKnownParams(prev, TIMESHEET_QUERY_KEYS, serialized);
      });
    },
    [setSearchParams, defaults, knownEmployeeIds, knownProjectIds, result.totalCount, pageSize],
  );

  /** CRUD refresh: не трогает URL, только инициирует пересчёт */
  const refresh = useCallback(() => {
    setRevision((r) => r + 1);
  }, []);

  return {
    entries: result.items,
    totalCount: result.totalCount,
    totals,
    loading: false,
    error,
    page: query.page,
    pageSize,
    filters,
    setPage,
    setFilters: setFiltersPartial,
    refresh,
  };
}

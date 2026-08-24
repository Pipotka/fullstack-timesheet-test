import { useState, useCallback, useMemo } from 'react';
import type { TimeEntryView } from '../../entities/time-entry/types';
import type { TimeEntryFilters, TimeEntryTotals } from '../../shared/api/contracts';
import type { ApiError } from '../../shared/types/api-error';
import { useRepositories } from '../../app/providers/useRepositories';

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
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [employeeId, setEmployeeId] = useState('');
  const [projectId, setProjectId] = useState('');
  const [page, setPage] = useState(1);
  const [error] = useState<ApiError | null>(null);
  // Force-update counter to re-compute data after mutations
  const [, setRevision] = useState(0);
  const pageSize = 10;

  const filters = useMemo<TimeEntryFilters>(
    () => ({ year, month, employeeId: employeeId || undefined, projectId: projectId || undefined }),
    [year, month, employeeId, projectId],
  );

  // Compute synchronously during render (mock data is synchronous)
  const result = repos.timeEntries.list(filters, page, pageSize);
  const totals = repos.timeEntries.getTotals(filters);

  const setFiltersPartial = useCallback((partial: Partial<TimeEntryFilters>) => {
    if (partial.year !== undefined) setYear(partial.year);
    if (partial.month !== undefined) setMonth(partial.month);
    if (partial.employeeId !== undefined) setEmployeeId(partial.employeeId);
    if (partial.projectId !== undefined) setProjectId(partial.projectId);
    setPage(1);
  }, [setYear, setMonth, setEmployeeId, setProjectId, setPage]);

  const refresh = useCallback(() => {
    setRevision((r) => r + 1);
  }, []);

  return {
    entries: result.items,
    totalCount: result.totalCount,
    totals,
    loading: false,
    error,
    page,
    pageSize,
    filters,
    setPage,
    setFilters: setFiltersPartial,
    refresh,
  };
}

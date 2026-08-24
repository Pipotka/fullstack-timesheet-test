import { useState, useCallback, useMemo, useEffect } from 'react';
import type { TimeEntryView } from '../../entities/time-entry/types';
import type { TimeEntryFilters, TimeEntryTotals } from '../../shared/api/contracts';
import type { ApiError } from '../../shared/types/api-error';
import { useRepositories } from '../../app/providers/RepositoriesProvider';

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
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<ApiError | null>(null);
  const [entries, setEntries] = useState<TimeEntryView[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totals, setTotals] = useState<TimeEntryTotals>({ hours: 0, amount: 0 });
  const pageSize = 10;

  const filters = useMemo<TimeEntryFilters>(
    () => ({ year, month, employeeId: employeeId || undefined, projectId: projectId || undefined }),
    [year, month, employeeId, projectId],
  );

  const refresh = useCallback(() => {
    setLoading(true);
    setError(null);
    try {
      const result = repos.timeEntries.list(filters, page, pageSize);
      const t = repos.timeEntries.getTotals(filters);
      setEntries(result.items);
      setTotalCount(result.totalCount);
      setTotals(t);
    } catch (e) {
      setError({ code: 'UNKNOWN', message: (e as Error).message });
    } finally {
      setLoading(false);
    }
  }, [repos, filters, page, pageSize]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const setFiltersPartial = useCallback((partial: Partial<TimeEntryFilters>) => {
    if (partial.year !== undefined) setYear(partial.year);
    if (partial.month !== undefined) setMonth(partial.month);
    if (partial.employeeId !== undefined) setEmployeeId(partial.employeeId);
    if (partial.projectId !== undefined) setProjectId(partial.projectId);
    setPage(1);
  }, []);

  return {
    entries,
    totalCount,
    totals,
    loading,
    error,
    page,
    pageSize,
    filters,
    setPage,
    setFilters: setFiltersPartial,
    refresh,
  };
}

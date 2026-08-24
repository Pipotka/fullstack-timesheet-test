import { useState, useCallback } from 'react';
import type { ProjectReport } from '../../entities/report/types';
import { useRepositories } from '../../app/providers/useRepositories';

export interface UseProjectReportResult {
  report: ProjectReport | null;
  loading: boolean;
  year: number;
  month: number;
  setYearMonth: (year: number, month: number) => void;
  refresh: () => void;
}

export function useProjectReport(): UseProjectReportResult {
  const repos = useRepositories();
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  // Force-update counter to re-compute data after mutations
  const [, setRevision] = useState(0);

  // Compute synchronously during render (mock data is synchronous)
  const report = repos.reports.getProjectReport(year, month);

  const setYearMonth = useCallback((y: number, m: number) => {
    setYear(y);
    setMonth(m);
  }, [setYear, setMonth]);

  const refresh = useCallback(() => {
    setRevision((r) => r + 1);
  }, []);

  return { report, loading: false, year, month, setYearMonth, refresh };
}

import { useState, useCallback, useEffect } from 'react';
import type { ProjectReport } from '../../entities/report/types';
import { useRepositories } from '../../app/providers/RepositoriesProvider';

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
  const [loading, setLoading] = useState(false);
  const [report, setReport] = useState<ProjectReport | null>(null);

  const refresh = useCallback(() => {
    setLoading(true);
    try {
      const result = repos.reports.getProjectReport(year, month);
      setReport(result);
    } finally {
      setLoading(false);
    }
  }, [repos, year, month]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const setYearMonth = useCallback((y: number, m: number) => {
    setYear(y);
    setMonth(m);
  }, []);

  return { report, loading, year, month, setYearMonth, refresh };
}

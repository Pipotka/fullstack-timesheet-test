import { useState, useCallback, useMemo, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import type { ProjectReport } from '../../entities/report/types';
import { useRepositories } from '../../app/providers/useRepositories';
import { getCurrentYearMonth } from '../../shared/lib/date-utils';
import {
  parseReportQuery,
  serializeReportQuery,
  replaceKnownParams,
  REPORT_QUERY_KEYS,
  type FilterDefaults,
  type ReportQuery,
} from '../../shared/lib/filter-query';

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
  const [searchParams, setSearchParams] = useSearchParams();

  // Defaults: текущие год/месяц
  const defaults = useMemo<FilterDefaults>(() => {
    const { year, month } = getCurrentYearMonth();
    return { year, month };
  }, []);

  // Парсим URL — источник истины
  const query: ReportQuery = useMemo(
    () => parseReportQuery(searchParams, defaults),
    [searchParams, defaults],
  );

  // Канонизация URL: если в URL есть невалидные значения, нормализуем их
  useEffect(() => {
    const canonical = serializeReportQuery(query, defaults);
    const merged = replaceKnownParams(searchParams, REPORT_QUERY_KEYS, canonical);

    // Сравниваем только known keys между текущим URL и канонической формой
    const currentKnownValues = REPORT_QUERY_KEYS.map((key) => searchParams.get(key)).join('|');
    const canonicalKnownValues = REPORT_QUERY_KEYS.map((key) => merged.get(key)).join('|');

    // Если known params отличаются, обновляем URL через replace
    if (currentKnownValues !== canonicalKnownValues) {
      setSearchParams(merged, { replace: true });
    }
  }, [searchParams, query, defaults, setSearchParams]);

  // Счётчик ревизий для пересчёта после мутаций
  const [, setRevision] = useState(0);

  // Синхронный расчёт отчёта (mock-данные синхронны)
  const report = repos.reports.getProjectReport(query.year, query.month);

  /** Изменение year/month → запись в URL, сохранение неизвестных ключей */
  const setYearMonth = useCallback(
    (year: number, month: number) => {
      setSearchParams((prev) => {
        const next: ReportQuery = { year, month };
        const serialized = serializeReportQuery(next, defaults);
        return replaceKnownParams(prev, REPORT_QUERY_KEYS, serialized);
      });
    },
    [setSearchParams, defaults],
  );

  /** CRUD refresh: не трогает URL */
  const refresh = useCallback(() => {
    setRevision((r) => r + 1);
  }, []);

  return {
    report,
    loading: false,
    year: query.year,
    month: query.month,
    setYearMonth,
    refresh,
  };
}

import { getMonthName } from '../../shared/lib/date-utils';
import { useProjectReport } from '../../features/reports/useProjectReport';
import { ProjectReportGrid } from '../../widgets/reports/ProjectReportGrid';

const YEARS = [2025, 2026, 2027];
const MONTHS = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: getMonthName(i + 1) }));

export function ProjectReportsPage() {
  const { report, loading, year, month, setYearMonth } = useProjectReport();

  return (
    <div className="container-fluid py-3">
      <h4 className="mb-3">Отчёт по проектам</h4>

      <div className="d-flex flex-wrap align-items-end gap-3 mb-3">
        <div>
          <label className="form-label mb-1 small text-muted">Месяц</label>
          <div className="d-flex gap-2">
            <select
              className="form-select form-select-sm"
              value={month}
              onChange={(e) => setYearMonth(year, Number(e.target.value))}
            >
              {MONTHS.map((m) => (
                <option key={m.value} value={m.value}>
                  {m.label}
                </option>
              ))}
            </select>
            <select
              className="form-select form-select-sm"
              style={{ width: 'auto' }}
              value={year}
              onChange={(e) => setYearMonth(Number(e.target.value), month)}
            >
              {YEARS.map((y) => (
                <option key={y} value={y}>
                  {y}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {report && (
        <ProjectReportGrid
          rows={report.rows}
          totalHours={report.totals.hours}
          totalAmount={report.totals.amount}
          loading={loading}
        />
      )}
    </div>
  );
}

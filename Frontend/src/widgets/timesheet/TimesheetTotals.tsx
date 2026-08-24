import type { TimeEntryTotals } from '../../shared/api/contracts';
import { formatMoney, formatHours } from '../../shared/lib/money-utils';

interface TimesheetTotalsProps {
  totals: TimeEntryTotals;
  totalCount: number;
  page: number;
  pageSize: number;
  onPageChange: (page: number) => void;
}

export function TimesheetTotals({
  totals,
  totalCount,
  page,
  pageSize,
  onPageChange,
}: TimesheetTotalsProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <div className="d-flex flex-wrap justify-content-between align-items-center mt-3 gap-2">
      <div className="d-flex gap-4">
        <span>
          <strong>Итого часов:</strong> {formatHours(totals.hours)}
        </span>
        <span>
          <strong>Итого стоимость:</strong> {formatMoney(totals.amount)}
        </span>
        <span className="text-muted small">
          Записей: {totalCount}
        </span>
      </div>

      {totalPages > 1 && (
        <nav>
          <ul className="pagination pagination-sm mb-0">
            <li className={`page-item ${page <= 1 ? 'disabled' : ''}`}>
              <button className="page-link" onClick={() => onPageChange(page - 1)}>
                ←
              </button>
            </li>
            <li className="page-item disabled">
              <span className="page-link">
                {page} / {totalPages}
              </span>
            </li>
            <li className={`page-item ${page >= totalPages ? 'disabled' : ''}`}>
              <button className="page-link" onClick={() => onPageChange(page + 1)}>
                →
              </button>
            </li>
          </ul>
        </nav>
      )}
    </div>
  );
}

import { useMemo } from 'react';
import { AgGridReact } from 'ag-grid-react';
import type { ColDef } from 'ag-grid-community';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';
import type { TimeEntryView } from '../../entities/time-entry/types';
import { formatDateRu } from '../../shared/lib/date-utils';
import { formatMoney, formatHours } from '../../shared/lib/money-utils';

interface TimesheetGridProps {
  entries: TimeEntryView[];
  loading: boolean;
  onEdit: (entry: TimeEntryView) => void;
  onDelete: (entry: TimeEntryView) => void;
}

export function TimesheetGrid({ entries, loading, onEdit, onDelete }: TimesheetGridProps) {
  const columnDefs = useMemo<ColDef<TimeEntryView>[]>(
    () => [
      {
        headerName: 'Дата',
        field: 'date',
        valueFormatter: (p) => formatDateRu(p.value as string),
        width: 110,
      },
      { headerName: 'Сотрудник', field: 'employeeName', flex: 1, minWidth: 150 },
      { headerName: 'Проект', field: 'projectCode', width: 90 },
      {
        headerName: 'Часы',
        field: 'hours',
        valueFormatter: (p) => formatHours(p.value as number),
        width: 80,
        cellClass: 'text-end',
      },
      {
        headerName: 'Ставка',
        field: 'rate',
        valueFormatter: (p) => formatMoney(p.value as number),
        width: 110,
        cellClass: 'text-end',
      },
      {
        headerName: 'Стоимость',
        field: 'amount',
        valueFormatter: (p) => formatMoney(p.value as number),
        width: 120,
        cellClass: 'text-end',
      },
      { headerName: 'Комментарий', field: 'comment', flex: 1, minWidth: 120 },
      {
        headerName: 'Переработка',
        field: 'isOvertime',
        valueFormatter: (p) => (p.value ? '⚠ Да' : ''),
        width: 110,
        cellClass: (p) => (p.value ? 'text-danger fw-bold' : ''),
      },
      {
        headerName: 'Действия',
        width: 140,
        sortable: false,
        filter: false,
        cellRenderer: (params: { data: TimeEntryView }) => (
          <div className="d-flex gap-1">
            <button
              className="btn btn-outline-primary btn-sm"
              onClick={() => onEdit(params.data)}
            >
              Изм.
            </button>
            <button
              className="btn btn-outline-danger btn-sm"
              onClick={() => onDelete(params.data)}
            >
              Удал.
            </button>
          </div>
        ),
      },
    ],
    [onEdit, onDelete],
  );

  const defaultColDef = useMemo<ColDef>(
    () => ({
      sortable: true,
      filter: false,
      resizable: true,
    }),
    [],
  );

  if (loading) {
    return <div className="text-center py-4 text-muted">Загрузка...</div>;
  }

  if (entries.length === 0) {
    return <div className="text-center py-4 text-muted">Нет записей за выбранный период</div>;
  }

  return (
    <div className="ag-theme-quartz" style={{ height: 400, width: '100%' }}>
      <AgGridReact
        rowData={entries}
        columnDefs={columnDefs}
        defaultColDef={defaultColDef}
        pagination={false}
        domLayout="normal"
        rowHeight={38}
        headerHeight={36}
      />
    </div>
  );
}

import { useMemo } from 'react';
import { AgGridReact } from 'ag-grid-react';
import type { ColDef } from 'ag-grid-community';
import { CellStyleModule, RowStyleModule } from 'ag-grid-community';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';
import type { ProjectReportRow } from '../../entities/report/types';
import { formatMoney, formatHours } from '../../shared/lib/money-utils';

interface ProjectReportGridProps {
  rows: ProjectReportRow[];
  totalHours: number;
  totalAmount: number;
  loading: boolean;
}

export function ProjectReportGrid({
  rows,
  totalHours,
  totalAmount,
  loading,
}: ProjectReportGridProps) {
  const columnDefs = useMemo<ColDef<ProjectReportRow>[]>(
    () => [
      {
        headerName: 'Проект',
        valueGetter: (p) => `${p.data?.projectCode} — ${p.data?.projectName}`,
        flex: 1,
        minWidth: 200,
      },
      {
        headerName: 'Часы',
        field: 'hours',
        valueFormatter: (p) => formatHours(p.value as number),
        width: 90,
        cellClass: 'text-end',
      },
      {
        headerName: 'Стоимость',
        field: 'amount',
        valueFormatter: (p) => formatMoney(p.value as number),
        width: 130,
        cellClass: 'text-end',
      },
      {
        headerName: 'Бюджет',
        field: 'budget',
        valueFormatter: (p) => formatMoney(p.value as number),
        width: 130,
        cellClass: 'text-end',
      },
      {
        headerName: 'Освоено',
        field: 'percent',
        valueFormatter: (p) => `${Math.round(p.value as number)}%`,
        width: 100,
        cellClass: (params) => {
          const val = params.value as number;
          if (val > 100) return 'text-end text-danger fw-bold';
          if (val > 80) return 'text-end text-warning fw-bold';
          return 'text-end';
        },
        cellStyle: (params) => {
          const val = params.value as number;
          if (val > 100) return { backgroundColor: '#f8d7da' };
          if (val > 80) return { backgroundColor: '#fff3cd' };
          return null;
        },
      },
      {
        headerName: 'Статус',
        width: 130,
        valueGetter: (p) => {
          if (!p.data) return '';
          if (p.data.isOverspent) return 'Перерасход';
          if (p.data.isRisk) return 'Риск';
          return '';
        },
        cellClass: (params) => {
          const val = params.value as string;
          if (val === 'Перерасход') return 'text-danger fw-bold';
          if (val === 'Риск') return 'text-warning fw-bold';
          return '';
        },
      },
    ],
    [],
  );

  const defaultColDef = useMemo<ColDef>(
    () => ({
      sortable: false,
      filter: false,
      resizable: true,
    }),
    [],
  );

  if (loading) {
    return <div className="text-center py-4 text-muted">Загрузка...</div>;
  }

  if (rows.length === 0) {
    return <div className="text-center py-4 text-muted">Нет данных за выбранный период</div>;
  }

  // Add totals row
  const allRows = [
    ...rows,
    {
      projectId: '__total__',
      projectCode: '',
      projectName: 'Итого',
      hours: totalHours,
      amount: totalAmount,
      budget: 0,
      percent: 0,
      isRisk: false,
      isOverspent: false,
    } as ProjectReportRow,
  ];

  return (
    <div className="ag-theme-quartz" style={{ height: 350, width: '100%' }}>
      <AgGridReact
        rowData={allRows}
        columnDefs={columnDefs}
        defaultColDef={defaultColDef}
        modules={[CellStyleModule, RowStyleModule]}
        theme="legacy"
        pagination={false}
        domLayout="normal"
        rowHeight={38}
        headerHeight={36}
        getRowStyle={(params) => {
          if (params.data?.projectId === '__total__') {
            return { fontWeight: 'bold', backgroundColor: '#f0f0f0' };
          }
          return undefined;
        }}
      />
    </div>
  );
}

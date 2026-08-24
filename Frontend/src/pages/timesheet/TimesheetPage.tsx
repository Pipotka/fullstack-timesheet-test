import { useState, useCallback } from 'react';
import type { TimeEntryView, TimeEntryDto, TimeEntryUpdateDto } from '../../entities/time-entry/types';
import { useTimesheetData } from '../../features/timesheet/useTimesheetData';
import { useTimeEntryActions } from '../../features/timesheet/useTimeEntryActions';
import { TimesheetFilters } from '../../widgets/timesheet/TimesheetFilters';
import { TimesheetGrid } from '../../widgets/timesheet/TimesheetGrid';
import { TimesheetTotals } from '../../widgets/timesheet/TimesheetTotals';
import { TimeEntryModal } from '../../widgets/timesheet/TimeEntryModal';

export function TimesheetPage() {
  const {
    entries,
    totalCount,
    totals,
    loading,
    error,
    page,
    pageSize,
    filters,
    setPage,
    setFilters,
    refresh,
  } = useTimesheetData();

  const [modalOpen, setModalOpen] = useState(false);
  const [editingEntry, setEditingEntry] = useState<TimeEntryView | null>(null);

  const onSuccess = useCallback(() => {
    setModalOpen(false);
    setEditingEntry(null);
    refresh();
  }, [refresh]);

  const { submitting, formError, createEntry, updateEntry, deleteEntry, clearError } =
    useTimeEntryActions(onSuccess);

  const handleAdd = useCallback(() => {
    setEditingEntry(null);
    clearError();
    setModalOpen(true);
  }, [clearError]);

  const handleEdit = useCallback(
    (entry: TimeEntryView) => {
      setEditingEntry(entry);
      clearError();
      setModalOpen(true);
    },
    [clearError],
  );

  const handleDelete = useCallback(
    (entry: TimeEntryView) => {
      if (window.confirm(`Удалить запись от ${entry.date} (${entry.hours} ч.)?`)) {
        deleteEntry(entry.id);
      }
    },
    [deleteEntry],
  );

  const handleSubmit = useCallback(
    (dto: TimeEntryDto | TimeEntryUpdateDto, id?: string): boolean => {
      if (id) {
        return updateEntry(id, dto as TimeEntryUpdateDto);
      }
      return createEntry(dto as TimeEntryDto);
    },
    [createEntry, updateEntry],
  );

  return (
    <div className="container-fluid py-3">
      <h4 className="mb-3">Табель</h4>

      <TimesheetFilters
        year={filters.year}
        month={filters.month}
        employeeId={filters.employeeId ?? ''}
        projectId={filters.projectId ?? ''}
        onFilterChange={setFilters}
        onAdd={handleAdd}
      />

      {error && (
        <div className="alert alert-danger py-2" role="alert">
          {error.message}
        </div>
      )}

      <TimesheetGrid
        entries={entries}
        loading={loading}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <TimesheetTotals
        totals={totals}
        totalCount={totalCount}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
      />

      <TimeEntryModal
        show={modalOpen}
        editingEntry={editingEntry}
        onClose={() => {
          setModalOpen(false);
          setEditingEntry(null);
          clearError();
        }}
        onSubmit={handleSubmit}
        submitting={submitting}
        formError={formError}
      />
    </div>
  );
}

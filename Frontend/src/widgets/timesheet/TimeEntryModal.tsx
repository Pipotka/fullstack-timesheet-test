import { useEffect } from 'react';
import { Formik, Form, ErrorMessage } from 'formik';
import type { TimeEntryView, TimeEntryDto, TimeEntryUpdateDto } from '../../entities/time-entry/types';
import type { Employee } from '../../entities/employee/types';
import type { Project } from '../../entities/project/types';
import type { ApiError } from '../../shared/types/api-error';
import { useRepositories } from '../../app/providers/useRepositories';
import {
  timeEntryValidationSchema,
  normalizeHoursInput,
  type TimeEntryFormValues,
} from '../../features/timesheet/timeEntryValidation';

interface TimeEntryModalProps {
  show: boolean;
  editingEntry: TimeEntryView | null;
  onClose: () => void;
  onSubmit: (dto: TimeEntryDto | TimeEntryUpdateDto, id?: string) => boolean;
  submitting: boolean;
  formError: ApiError | null;
}

export function TimeEntryModal({
  show,
  editingEntry,
  onClose,
  onSubmit,
  submitting,
  formError,
}: TimeEntryModalProps) {
  const repos = useRepositories();
  const employees: Employee[] = repos.employees.getAll();
  const projects: Project[] = repos.projects.getAll();

  // Use the IDs directly from the entry (no fragile name/code lookup)
  const initialValues: TimeEntryFormValues = (() => {
    if (!editingEntry) {
      return { employeeId: '', projectId: '', date: '', hours: '', comment: '' };
    }
    return {
      employeeId: editingEntry.employeeId,
      projectId: editingEntry.projectId,
      date: editingEntry.date,
      hours: String(editingEntry.hours),
      comment: editingEntry.comment,
    };
  })();

  useEffect(() => {
    // Close on Escape
    if (!show) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [show, onClose]);

  if (!show) return null;

  return (
    <>
      <div className="modal-backdrop fade show" onClick={onClose} />
      <div
        className="modal fade show d-block"
        tabIndex={-1}
        role="dialog"
        aria-modal="true"
      >
        <div className="modal-dialog" role="document">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">
                {editingEntry ? 'Редактировать запись' : 'Новая запись'}
              </h5>
              <button
                type="button"
                className="btn-close"
                aria-label="Закрыть"
                onClick={onClose}
              />
            </div>

            <Formik
              initialValues={initialValues}
              validationSchema={timeEntryValidationSchema}
              enableReinitialize
              onSubmit={(values) => {
                const hours = Number(normalizeHoursInput(values.hours));
                if (editingEntry) {
                  const dto: TimeEntryUpdateDto = {
                    employeeId: values.employeeId,
                    projectId: values.projectId,
                    date: values.date,
                    hours,
                    comment: values.comment,
                    version: editingEntry.version,
                  };
                  onSubmit(dto, editingEntry.id);
                } else {
                  const dto: TimeEntryDto = {
                    employeeId: values.employeeId,
                    projectId: values.projectId,
                    date: values.date,
                    hours,
                    comment: values.comment,
                  };
                  onSubmit(dto);
                }
              }}
            >
              {({ handleSubmit, handleChange, values, touched, errors, setFieldValue }) => (
                <Form onSubmit={handleSubmit}>
                  <div className="modal-body">
                    {formError && (
                      <div className="alert alert-danger py-2" role="alert">
                        <strong>{formError.code}:</strong> {formError.message}
                      </div>
                    )}

                    <div className="mb-3">
                      <label className="form-label">Сотрудник</label>
                      <select
                        className={`form-select ${touched.employeeId && errors.employeeId ? 'is-invalid' : ''}`}
                        name="employeeId"
                        value={values.employeeId}
                        onChange={handleChange}
                      >
                        <option value="">— выберите —</option>
                        {employees.map((emp) => (
                          <option key={emp.id} value={emp.id}>
                            {emp.name}
                          </option>
                        ))}
                      </select>
                      <ErrorMessage
                        name="employeeId"
                        component="div"
                        className="invalid-feedback"
                      />
                    </div>

                    <div className="mb-3">
                      <label className="form-label">Проект</label>
                      <select
                        className={`form-select ${touched.projectId && errors.projectId ? 'is-invalid' : ''}`}
                        name="projectId"
                        value={values.projectId}
                        onChange={handleChange}
                      >
                        <option value="">— выберите —</option>
                        {projects.map((p) => (
                          <option key={p.id} value={p.id}>
                            {p.code} — {p.name}
                          </option>
                        ))}
                      </select>
                      <ErrorMessage
                        name="projectId"
                        component="div"
                        className="invalid-feedback"
                      />
                    </div>

                    <div className="mb-3">
                      <label className="form-label">Дата</label>
                      <input
                        type="date"
                        className={`form-control ${touched.date && errors.date ? 'is-invalid' : ''}`}
                        name="date"
                        value={values.date}
                        onChange={handleChange}
                      />
                      <ErrorMessage name="date" component="div" className="invalid-feedback" />
                    </div>

                    <div className="mb-3">
                      <label className="form-label">Часы</label>
                      <input
                        type="text"
                        inputMode="decimal"
                        className={`form-control ${touched.hours && errors.hours ? 'is-invalid' : ''}`}
                        name="hours"
                        value={values.hours}
                        onChange={(e) => {
                          const normalized = normalizeHoursInput(e.target.value);
                          setFieldValue('hours', normalized);
                        }}
                        placeholder="Например, 8 или 4.5"
                      />
                      <ErrorMessage name="hours" component="div" className="invalid-feedback" />
                    </div>

                    <div className="mb-3">
                      <label className="form-label">Комментарий</label>
                      <textarea
                        className="form-control"
                        name="comment"
                        value={values.comment}
                        onChange={handleChange}
                        rows={2}
                      />
                    </div>
                  </div>

                  <div className="modal-footer">
                    <button
                      type="button"
                      className="btn btn-secondary"
                      onClick={onClose}
                      disabled={submitting}
                    >
                      Отмена
                    </button>
                    <button type="submit" className="btn btn-primary" disabled={submitting}>
                      {submitting ? 'Сохранение...' : 'Сохранить'}
                    </button>
                  </div>
                </Form>
              )}
            </Formik>
          </div>
        </div>
      </div>
    </>
  );
}

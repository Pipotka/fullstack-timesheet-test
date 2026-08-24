import { useState, useCallback } from 'react';
import type { TimeEntryDto, TimeEntryUpdateDto } from '../../entities/time-entry/types';
import type { ApiError } from '../../shared/types/api-error';
import { useRepositories } from '../../app/providers/RepositoriesProvider';

export interface UseTimeEntryActionsResult {
  submitting: boolean;
  formError: ApiError | null;
  createEntry: (dto: TimeEntryDto) => boolean;
  updateEntry: (id: string, dto: TimeEntryUpdateDto) => boolean;
  deleteEntry: (id: string) => boolean;
  clearError: () => void;
}

export function useTimeEntryActions(onSuccess: () => void): UseTimeEntryActionsResult {
  const repos = useRepositories();
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<ApiError | null>(null);

  const createEntry = useCallback(
    (dto: TimeEntryDto): boolean => {
      setSubmitting(true);
      setFormError(null);
      try {
        const result = repos.timeEntries.create(dto);
        if (!result.ok) {
          setFormError(result.error);
          return false;
        }
        onSuccess();
        return true;
      } finally {
        setSubmitting(false);
      }
    },
    [repos, onSuccess],
  );

  const updateEntry = useCallback(
    (id: string, dto: TimeEntryUpdateDto): boolean => {
      setSubmitting(true);
      setFormError(null);
      try {
        const result = repos.timeEntries.update(id, dto);
        if (!result.ok) {
          setFormError(result.error);
          return false;
        }
        onSuccess();
        return true;
      } finally {
        setSubmitting(false);
      }
    },
    [repos, onSuccess],
  );

  const deleteEntry = useCallback(
    (id: string): boolean => {
      setSubmitting(true);
      setFormError(null);
      try {
        const result = repos.timeEntries.delete(id);
        if (!result.ok) {
          setFormError(result.error);
          return false;
        }
        onSuccess();
        return true;
      } finally {
        setSubmitting(false);
      }
    },
    [repos, onSuccess],
  );

  const clearError = useCallback(() => setFormError(null), []);

  return { submitting, formError, createEntry, updateEntry, deleteEntry, clearError };
}

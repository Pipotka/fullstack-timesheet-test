import { useContext } from 'react';
import type { Repositories } from '../../shared/api/contracts';
import { RepositoriesContext } from './RepositoriesContext';

export function useRepositories(): Repositories {
  const ctx = useContext(RepositoriesContext);
  if (!ctx) {
    throw new Error('useRepositories must be used within RepositoriesProvider');
  }
  return ctx;
}

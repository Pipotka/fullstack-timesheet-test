import { useMemo } from 'react';
import type { ReactNode } from 'react';
import { createRepositories } from '../../shared/api/mock/repositories';
import { RepositoriesContext } from './RepositoriesContext';

export function RepositoriesProvider({ children }: { children: ReactNode }) {
  const repos = useMemo(() => createRepositories(), []);
  return (
    <RepositoriesContext.Provider value={repos}>
      {children}
    </RepositoriesContext.Provider>
  );
}

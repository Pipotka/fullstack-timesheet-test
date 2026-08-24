import { createContext, useContext, useMemo } from 'react';
import type { ReactNode } from 'react';
import type { Repositories } from '../../shared/api/contracts';
import { createRepositories } from '../../shared/api/mock/repositories';

const RepositoriesContext = createContext<Repositories | null>(null);

export function RepositoriesProvider({ children }: { children: ReactNode }) {
  const repos = useMemo(() => createRepositories(), []);
  return (
    <RepositoriesContext.Provider value={repos}>
      {children}
    </RepositoriesContext.Provider>
  );
}

export function useRepositories(): Repositories {
  const ctx = useContext(RepositoriesContext);
  if (!ctx) {
    throw new Error('useRepositories must be used within RepositoriesProvider');
  }
  return ctx;
}

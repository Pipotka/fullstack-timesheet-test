import { createContext } from 'react';
import type { Repositories } from '../../shared/api/contracts';

export const RepositoriesContext = createContext<Repositories | null>(null);

import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import 'bootstrap/dist/css/bootstrap.min.css';
import './app/styles/global.css';
import { RepositoriesProvider } from './app/providers/RepositoriesProvider';
import { App } from './app/App';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RepositoriesProvider>
      <App />
    </RepositoriesProvider>
  </StrictMode>,
);

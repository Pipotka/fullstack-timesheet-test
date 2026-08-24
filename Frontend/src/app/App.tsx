import { BrowserRouter, Routes, Route, Link, useLocation } from 'react-router-dom';
import { TimesheetPage } from '../pages/timesheet/TimesheetPage';
import { ProjectReportsPage } from '../pages/reports/ProjectReportsPage';

function AppNav() {
  const location = useLocation();
  const isActive = (path: string) => location.pathname.startsWith(path);

  return (
    <nav className="navbar navbar-expand navbar-light bg-white border-bottom px-3">
      <span className="navbar-brand mb-0 h1">Учёт трудозатрат</span>
      <ul className="navbar-nav gap-2">
        <li className="nav-item">
          <Link
            className={`nav-link ${isActive('/timesheet') ? 'active fw-bold' : ''}`}
            to="/timesheet"
          >
            Табель
          </Link>
        </li>
        <li className="nav-item">
          <Link
            className={`nav-link ${isActive('/reports') ? 'active fw-bold' : ''}`}
            to="/reports/projects"
          >
            Отчёт по проектам
          </Link>
        </li>
      </ul>
    </nav>
  );
}

function NotFoundPage() {
  return (
    <div className="container py-5 text-center">
      <h3>Страница не найдена</h3>
      <p className="text-muted">
        Перейдите на <Link to="/timesheet">Табель</Link> или{' '}
        <Link to="/reports/projects">Отчёт по проектам</Link>.
      </p>
    </div>
  );
}

function HomePage() {
  return (
    <div className="container py-5 text-center">
      <h3>Добро пожаловать</h3>
      <p className="text-muted">Выберите раздел в навигации выше.</p>
    </div>
  );
}

export function App() {
  return (
    <BrowserRouter>
      <AppNav />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/timesheet" element={<TimesheetPage />} />
        <Route path="/reports/projects" element={<ProjectReportsPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </BrowserRouter>
  );
}

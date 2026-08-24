import type { Employee } from '../../../entities/employee/types';
import type { Project } from '../../../entities/project/types';
import type { TimeEntry } from '../../../entities/time-entry/types';
import type { ClosedPeriod } from '../../../entities/closed-period/types';

export const seedEmployees: Employee[] = [
  {
    id: 'emp-1',
    name: 'Иванов И. И.',
    department: 'Проектный',
    rates: [
      { from: '2026-01-01', value: 500 },
      { from: '2026-03-01', value: 600 },
    ],
  },
  {
    id: 'emp-2',
    name: 'Петрова А. С.',
    department: 'Проектный',
    rates: [
      { from: '2026-02-01', value: 700 },
    ],
  },
];

export const seedProjects: Project[] = [
  {
    id: 'proj-1',
    code: 'П-001',
    name: 'Реконструкция цеха',
    budget: 20000,
    startDate: '2026-01-01',
    endDate: '2026-03-31',
  },
  {
    id: 'proj-2',
    code: 'П-002',
    name: 'Инженерные сети',
    budget: 5000,
    startDate: '2026-03-01',
    endDate: null,
  },
];

export const seedTimeEntries: TimeEntry[] = [
  {
    id: 'te-1',
    employeeId: 'emp-1',
    projectId: 'proj-1',
    date: '2026-02-20',
    hours: 8,
    comment: 'Работа над документацией',
    version: 1,
  },
  {
    id: 'te-2',
    employeeId: 'emp-1',
    projectId: 'proj-1',
    date: '2026-03-05',
    hours: 8,
    comment: 'Проектирование',
    version: 1,
  },
  {
    id: 'te-3',
    employeeId: 'emp-2',
    projectId: 'proj-1',
    date: '2026-03-05',
    hours: 4,
    comment: 'Расчёты',
    version: 1,
  },
  {
    id: 'te-4',
    employeeId: 'emp-2',
    projectId: 'proj-2',
    date: '2026-03-06',
    hours: 10,
    comment: 'Инженерные изыскания',
    version: 1,
  },
];

export const seedClosedPeriods: ClosedPeriod[] = [];

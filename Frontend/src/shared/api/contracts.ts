import type { Employee } from '../../entities/employee/types';
import type { Project } from '../../entities/project/types';
import type {
  TimeEntryDto,
  TimeEntryUpdateDto,
  TimeEntryView,
} from '../../entities/time-entry/types';
import type { ProjectReport } from '../../entities/report/types';
import type { PaginatedResult } from '../types/pagination';
import type { ApiError } from '../types/api-error';

export interface TimeEntryFilters {
  year: number;
  month: number;
  employeeId?: string;
  projectId?: string;
}

export interface TimeEntryTotals {
  hours: number;
  amount: number;
}

export interface EmployeeRepository {
  getAll(): Employee[];
  getById(id: string): Employee | undefined;
}

export interface ProjectRepository {
  getAll(): Project[];
  getById(id: string): Project | undefined;
}

export interface TimeEntryRepository {
  list(filters: TimeEntryFilters, page: number, pageSize: number): PaginatedResult<TimeEntryView>;
  getTotals(filters: TimeEntryFilters): TimeEntryTotals;
  create(dto: TimeEntryDto): { ok: true; entry: TimeEntryView } | { ok: false; error: ApiError };
  update(id: string, dto: TimeEntryUpdateDto): { ok: true; entry: TimeEntryView } | { ok: false; error: ApiError };
  delete(id: string): { ok: true } | { ok: false; error: ApiError };
}

export interface ClosedPeriodRepository {
  isClosed(year: number, month: number): boolean;
  close(year: number, month: number): void;
  open(year: number, month: number): void;
}

export interface ReportRepository {
  getProjectReport(year: number, month: number): ProjectReport;
}

export interface Repositories {
  employees: EmployeeRepository;
  projects: ProjectRepository;
  timeEntries: TimeEntryRepository;
  closedPeriods: ClosedPeriodRepository;
  reports: ReportRepository;
}

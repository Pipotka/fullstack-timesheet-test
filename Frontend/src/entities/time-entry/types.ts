import type { DateString } from '../rate/types';

export interface TimeEntry {
  id: string;
  employeeId: string;
  projectId: string;
  date: DateString;
  hours: number;
  comment: string;
  version: number;
}

export interface TimeEntryDto {
  employeeId: string;
  projectId: string;
  date: DateString;
  hours: number;
  comment: string;
}

export interface TimeEntryUpdateDto extends TimeEntryDto {
  version: number;
}

export interface TimeEntryView {
  id: string;
  employeeId: string;
  projectId: string;
  employeeName: string;
  projectCode: string;
  date: DateString;
  hours: number;
  rate: number;
  amount: number;
  comment: string;
  isOvertime: boolean;
  version: number;
}

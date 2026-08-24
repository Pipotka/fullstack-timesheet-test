import type { DateString } from '../rate/types';

export interface Project {
  id: string;
  code: string;
  name: string;
  budget: number;
  startDate: DateString;
  /** null = бессрочный */
  endDate: DateString | null;
}

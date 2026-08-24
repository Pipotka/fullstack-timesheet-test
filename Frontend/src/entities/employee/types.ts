import type { Rate } from '../rate/types';

export interface Employee {
  id: string;
  name: string;
  department: string;
  rates: Rate[];
}

export interface ProjectReportRow {
  projectId: string;
  projectCode: string;
  projectName: string;
  hours: number;
  amount: number;
  budget: number;
  percent: number;
  isRisk: boolean;
  isOverspent: boolean;
}

export interface ProjectReportTotals {
  hours: number;
  amount: number;
}

export interface ProjectReport {
  rows: ProjectReportRow[];
  totals: ProjectReportTotals;
}

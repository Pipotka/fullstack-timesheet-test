/**
 * Чистые функции подсветки ячеек отчёта проектов.
 * Используют точные признаки isOverspent / isRisk,
 * чтобы подсветка совпадала со статусом, а не с округлённым процентом.
 */

export interface ReportRowFlags {
  isOverspent?: boolean;
  isRisk?: boolean;
}

export interface PercentCellHighlight {
  cssClass: string;
  backgroundColor: string | null;
}

const NORMAL: PercentCellHighlight = {
  cssClass: 'text-end',
  backgroundColor: null,
};

const RISK: PercentCellHighlight = {
  cssClass: 'text-end text-warning fw-bold',
  backgroundColor: '#fff3cd',
};

const OVERSPENT: PercentCellHighlight = {
  cssClass: 'text-end text-danger fw-bold',
  backgroundColor: '#f8d7da',
};

/**
 * Возвращает CSS-класс и фоновый цвет для ячейки «Освоено %».
 * Приоритет: перерасход > риск > норма.
 */
export function getPercentCellHighlight(data: ReportRowFlags | null | undefined): PercentCellHighlight {
  if (data?.isOverspent) return OVERSPENT;
  if (data?.isRisk) return RISK;
  return NORMAL;
}

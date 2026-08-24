import type { DateString } from '../../entities/rate/types';

/** Форматирует Date в yyyy-MM-dd */
export function toDateStr(d: Date): DateString {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

/** Парсит yyyy-MM-dd в Date (локальная дата, без UTC) */
export function parseDateStr(s: DateString): Date {
  const [y, m, d] = s.split('-').map(Number);
  return new Date(y, m - 1, d);
}

/** Форматирует yyyy-MM-dd в DD.MM.YYYY для отображения */
export function formatDateRu(s: DateString): string {
  const [y, m, d] = s.split('-');
  return `${d}.${m}.${y}`;
}

/** Возвращает yyyy-MM-dd первого и последнего дня месяца */
export function getMonthBounds(year: number, month: number): { from: DateString; to: DateString } {
  const from = new Date(year, month - 1, 1);
  const to = new Date(year, month, 0);
  return { from: toDateStr(from), to: toDateStr(to) };
}

/** Проверяет, что дата попадает в [from, to] включительно */
export function isDateInRange(date: DateString, from: DateString, to: DateString | null): boolean {
  if (date < from) return false;
  if (to !== null && date > to) return false;
  return true;
}

/** Названия месяцев на русском */
const MONTH_NAMES = [
  'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
  'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь',
];

export function getMonthName(month: number): string {
  return MONTH_NAMES[month - 1] ?? '';
}

/** Текущий год/месяц */
export function getCurrentYearMonth(): { year: number; month: number } {
  const now = new Date();
  return { year: now.getFullYear(), month: now.getMonth() + 1 };
}

/** Форматирует число как сумму в рублях с копейками */
export function formatMoney(value: number): string {
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

/** Форматирует число часов */
export function formatHours(value: number): string {
  return new Intl.NumberFormat('ru-RU', {
    minimumFractionDigits: value % 1 === 0 ? 0 : 1,
    maximumFractionDigits: 1,
  }).format(value);
}

/** Округляет до копеек */
export function roundMoney(value: number): number {
  return Math.round(value * 100) / 100;
}

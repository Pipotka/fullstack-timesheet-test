/**
 * Модуль для синхронизации фильтров с URL (query-параметры).
 * Не имеет зависимостей от React.
 */

// ─── Типы ────────────────────────────────────────────────────────────────────

/** Значения по умолчанию для year/month (обычно текущие). */
export interface FilterDefaults {
  year: number;
  month: number;
}

/** Состояние фильтров табеля, восстановимое из URL. */
export interface TimesheetQuery {
  year: number;
  month: number;
  employeeId: string; // '' = без фильтра
  projectId: string;  // '' = без фильтра
  page: number;
}

/** Состояние фильтров отчёта, восстановимое из URL. */
export interface ReportQuery {
  year: number;
  month: number;
}

// ─── Константы ───────────────────────────────────────────────────────────────

/** Известные ключи query-параметров для каждого маршрута. */
export const TIMESHEET_QUERY_KEYS = ['year', 'month', 'employeeId', 'projectId', 'page'] as const;
export const REPORT_QUERY_KEYS = ['year', 'month'] as const;

// ─── Вспомогательные функции ─────────────────────────────────────────────────

/**
 * Парсит строку в положительное безопасное целое.
 * Отклоняет: пустые строки, "12abc", дроби ("1.5"), NaN, Infinity, 0, отрицательные.
 */
function parseSafePositiveInt(raw: string | null): number | null {
  if (raw === null || raw === '') return null;
  // Только цифры — отклоняет "1.5", "12abc", "NaN", "Infinity", "-1", "1e2"
  if (!/^\d+$/.test(raw)) return null;
  const n = Number(raw);
  if (!Number.isSafeInteger(n) || n <= 0) return null;
  return n;
}

/**
 * Проверяет, что ID присутствует в множестве известных.
 * Возвращает ID если известен, иначе '' (нет фильтра).
 */
export function validateKnownId(id: string, knownIds: ReadonlySet<string>): string {
  if (id !== '' && knownIds.has(id)) return id;
  return '';
}

// ─── Парсеры ─────────────────────────────────────────────────────────────────

/**
 * Парсит URLSearchParams в TimesheetQuery.
 * Не мутирует вход.
 * - year: положительное безопасное целое, ≤ currentYear (иначе нормализация); default = defaults.year
 * - month: целое 1..12; вне диапазона → default
 * - page: положительное безопасное целое; невалидное → 1
 * - employeeId/projectId: если неизвестны → ''
 */
export function parseTimesheetQuery(
  params: URLSearchParams,
  defaults: FilterDefaults,
  knownEmployeeIds: ReadonlySet<string>,
  knownProjectIds: ReadonlySet<string>,
): TimesheetQuery {
  // Year
  const rawYear = params.get('year');
  let year: number;
  if (rawYear !== null) {
    const parsed = parseSafePositiveInt(rawYear);
    if (parsed !== null) {
      year = parsed > defaults.year ? defaults.year : parsed;
    } else {
      year = defaults.year;
    }
  } else {
    year = defaults.year;
  }

  // Month
  const rawMonth = params.get('month');
  let month: number;
  if (rawMonth !== null) {
    const parsed = parseSafePositiveInt(rawMonth);
    if (parsed !== null && parsed >= 1 && parsed <= 12) {
      month = parsed;
    } else {
      month = defaults.month;
    }
  } else {
    month = defaults.month;
  }

  // EmployeeId
  const rawEmployeeId = params.get('employeeId');
  const employeeId = rawEmployeeId !== null ? validateKnownId(rawEmployeeId, knownEmployeeIds) : '';

  // ProjectId
  const rawProjectId = params.get('projectId');
  const projectId = rawProjectId !== null ? validateKnownId(rawProjectId, knownProjectIds) : '';

  // Page
  const rawPage = params.get('page');
  let page: number;
  if (rawPage !== null) {
    const parsed = parseSafePositiveInt(rawPage);
    page = parsed !== null ? parsed : 1;
  } else {
    page = 1;
  }

  return { year, month, employeeId, projectId, page };
}

/**
 * Парсит URLSearchParams в ReportQuery.
 * Не мутирует вход.
 */
export function parseReportQuery(
  params: URLSearchParams,
  defaults: FilterDefaults,
): ReportQuery {
  // Year
  const rawYear = params.get('year');
  let year: number;
  if (rawYear !== null) {
    const parsed = parseSafePositiveInt(rawYear);
    if (parsed !== null) {
      year = parsed > defaults.year ? defaults.year : parsed;
    } else {
      year = defaults.year;
    }
  } else {
    year = defaults.year;
  }

  // Month
  const rawMonth = params.get('month');
  let month: number;
  if (rawMonth !== null) {
    const parsed = parseSafePositiveInt(rawMonth);
    if (parsed !== null && parsed >= 1 && parsed <= 12) {
      month = parsed;
    } else {
      month = defaults.month;
    }
  } else {
    month = defaults.month;
  }

  return { year, month };
}

// ─── Сериализаторы ───────────────────────────────────────────────────────────

/**
 * Сериализует TimesheetQuery в URLSearchParams (компактная форма).
 * Опускает значения, совпадающие с defaults.
 * Выход содержит только известные ключи.
 */
export function serializeTimesheetQuery(
  query: TimesheetQuery,
  defaults: FilterDefaults,
): URLSearchParams {
  const params = new URLSearchParams();

  if (query.year !== defaults.year) {
    params.set('year', String(query.year));
  }
  if (query.month !== defaults.month) {
    params.set('month', String(query.month));
  }
  if (query.employeeId !== '') {
    params.set('employeeId', query.employeeId);
  }
  if (query.projectId !== '') {
    params.set('projectId', query.projectId);
  }
  if (query.page !== 1) {
    params.set('page', String(query.page));
  }

  return params;
}

/**
 * Сериализует ReportQuery в URLSearchParams (компактная форма).
 * Опускает значения, совпадающие с defaults.
 */
export function serializeReportQuery(
  query: ReportQuery,
  defaults: FilterDefaults,
): URLSearchParams {
  const params = new URLSearchParams();

  if (query.year !== defaults.year) {
    params.set('year', String(query.year));
  }
  if (query.month !== defaults.month) {
    params.set('month', String(query.month));
  }

  return params;
}

// ─── Merge / Patch ───────────────────────────────────────────────────────────

/**
 * Создаёт новые URLSearchParams: заменяет известные ключи значениями из serialized,
 * сохраняя все неизвестные ключи из existing. Не мутирует вход.
 */
export function replaceKnownParams(
  existing: URLSearchParams,
  knownKeys: readonly string[],
  serialized: URLSearchParams,
): URLSearchParams {
  const knownSet = new Set(knownKeys);
  const result = new URLSearchParams();

  // Сохраняем неизвестные ключи из existing
  for (const [key, value] of existing.entries()) {
    if (!knownSet.has(key)) {
      result.set(key, value);
    }
  }

  // Добавляем все ключи из serialized
  for (const [key, value] of serialized.entries()) {
    result.set(key, value);
  }

  return result;
}

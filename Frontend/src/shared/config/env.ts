/**
 * Runtime-конфигурация окружения.
 *
 * Модуль читает переменную VITE_API_BASE_URL из import.meta.env,
 * нормализует значение (trim) и пробрасывает понятную ошибку,
 * если переменная отсутствует или пуста после обрезки пробелов.
 *
 * Импортируется только там, где действительно нужен базовый URL API
 * (например, в HTTP-клиенте). Mock-репозитории и UI не зависят от этого модуля.
 */

import { validateApiBaseUrl } from './env-validation';

// Безопасное чтение import.meta.env: вне Vite (например, в Node/tsx)
// import.meta.env может быть undefined, поэтому используем optional chaining.
const rawValue: unknown =
  (import.meta as { env?: Record<string, unknown> }).env?.VITE_API_BASE_URL;

/** Базовый URL backend API, полученный из переменных окружения Vite. */
export const apiBaseUrl: string = validateApiBaseUrl(rawValue);

// Ре-экспорт для удобства
export { validateApiBaseUrl } from './env-validation';

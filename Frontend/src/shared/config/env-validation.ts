/**
 * Чистая функция валидации базового URL API.
 * Не зависит от import.meta.env и может тестироваться вне Vite.
 */

/**
 * Проверяет и нормализует значение базового URL API.
 * @param rawValue — сырое значение из import.meta.env
 * @returns обрезанная непустая строка
 * @throws Error если значение отсутствует или пустое после trim
 */
export function validateApiBaseUrl(rawValue: unknown): string {
  const trimmed = typeof rawValue === 'string' ? rawValue.trim() : '';
  if (trimmed.length === 0) {
    throw new Error(
      'Переменная окружения VITE_API_BASE_URL не задана или пуста. ' +
        'Создайте файл .env на основе .env.example и укажите базовый URL API.',
    );
  }
  return trimmed;
}

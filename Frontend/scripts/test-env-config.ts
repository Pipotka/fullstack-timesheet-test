/**
 * Регрессионные тесты для env-конфигурации.
 *
 * Проверяет:
 *  — наличие и содержимое .env.example;
 *  — правила .gitignore для env-файлов;
 *  — поведение validateApiBaseUrl при разных входных значениях.
 *
 * Запуск:  npx tsx scripts/test-env-config.ts
 */

import { readFileSync, existsSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { execSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { validateApiBaseUrl } from '../src/shared/config/env-validation';

// ─── Инфраструктура ──────────────────────────────────────────────────────────

let passed = 0;
let failed = 0;

function assert(condition: boolean, name: string, detail?: string): void {
  if (condition) {
    console.log(`  PASS  ${name}`);
    passed++;
  } else {
    console.error(`  FAIL  ${name}${detail ? ' — ' + detail : ''}`);
    failed++;
  }
}

const FRONTEND_ROOT = resolve(dirname(fileURLToPath(import.meta.url)), '..');

// ─── .env.example ────────────────────────────────────────────────────────────

console.log('\n── .env.example ──');

const envExamplePath = resolve(FRONTEND_ROOT, '.env.example');
assert(existsSync(envExamplePath), '.env.example существует');

const envExampleContent = readFileSync(envExamplePath, 'utf-8');
assert(
  envExampleContent.includes('VITE_API_BASE_URL=http://localhost:5000'),
  '.env.example содержит VITE_API_BASE_URL=http://localhost:5000',
);

assert(
  !envExampleContent.includes('password') && !envExampleContent.includes('secret'),
  '.env.example не содержит секретов',
);

// ─── .gitignore rules ────────────────────────────────────────────────────────

console.log('\n── .gitignore rules ──');

const gitignorePath = resolve(FRONTEND_ROOT, '.gitignore');
assert(existsSync(gitignorePath), '.gitignore существует');

const gitignoreContent = readFileSync(gitignorePath, 'utf-8');
assert(gitignoreContent.includes('.env'), '.gitignore содержит правило .env');
assert(gitignoreContent.includes('.env.*'), '.gitignore содержит правило .env.*');
assert(gitignoreContent.includes('!.env.example'), '.gitignore содержит исключение !.env.example');

// Проверяем, что !.env.example стоит ПОСЛЕ .env.*
const envStarIndex = gitignoreContent.indexOf('.env.*');
const negExampleIndex = gitignoreContent.indexOf('!.env.example');
assert(
  negExampleIndex > envStarIndex,
  '!.env.example стоит после .env.*',
);

// Проверяем через git check-ignore, что файлы игнорируются
function isGitIgnored(filePath: string): boolean {
  try {
    execSync(`git check-ignore -q "${filePath}"`, {
      cwd: resolve(FRONTEND_ROOT, '..'),
      stdio: 'pipe',
    });
    return true;
  } catch {
    return false;
  }
}

// Создаём временные файлы для проверки ignore
const tempFiles = ['.env', '.env.development', '.env.production', 'env.local'];
for (const f of tempFiles) {
  const fullPath = resolve(FRONTEND_ROOT, f);
  if (!existsSync(fullPath)) {
    execSync(`touch "${fullPath}"`, { stdio: 'pipe' });
  }
}

assert(isGitIgnored('Frontend/.env'), 'Frontend/.env игнорируется git');
assert(isGitIgnored('Frontend/.env.development'), 'Frontend/.env.development игнорируется git');
assert(isGitIgnored('Frontend/.env.production'), 'Frontend/.env.production игнорируется git');
assert(isGitIgnored('Frontend/env.local'), 'Frontend/env.local игнорируется git');
assert(!isGitIgnored('Frontend/.env.example'), 'Frontend/.env.example НЕ игнорируется git');

// Удаляем временные файлы
for (const f of tempFiles) {
  const fullPath = resolve(FRONTEND_ROOT, f);
  if (existsSync(fullPath)) {
    execSync(`rm -f "${fullPath}"`, { stdio: 'pipe' });
  }
}

// ─── validateApiBaseUrl ──────────────────────────────────────────────────────

console.log('\n── validateApiBaseUrl ──');

// Valid values
assert(
  validateApiBaseUrl('http://localhost:5000') === 'http://localhost:5000',
  'valid URL returned as-is',
);

assert(
  validateApiBaseUrl('  http://localhost:5000  ') === 'http://localhost:5000',
  'URL with surrounding whitespace is trimmed',
);

assert(
  validateApiBaseUrl('https://api.example.com/v1') === 'https://api.example.com/v1',
  'HTTPS URL accepted',
);

// Invalid values — should throw
function expectThrow(value: unknown, name: string): void {
  try {
    validateApiBaseUrl(value);
    assert(false, name, 'ожидалась ошибка, но функция вернула значение');
  } catch (err) {
    const message = err instanceof Error ? err.message : String(err);
    assert(
      message.includes('VITE_API_BASE_URL'),
      name,
      message.includes('VITE_API_BASE_URL') ? undefined : `сообщение не содержит VITE_API_BASE_URL: ${message}`,
    );
  }
}

expectThrow('', 'empty string throws');
expectThrow('   ', 'whitespace-only string throws');
expectThrow('\t\n', 'tabs and newlines only throws');
expectThrow(undefined, 'undefined throws');
expectThrow(null, 'null throws');
expectThrow(42, 'number throws');
expectThrow({}, 'object throws');

// ─── vite-env.d.ts ───────────────────────────────────────────────────────────

console.log('\n── vite-env.d.ts ──');

const viteEnvDtsPath = resolve(FRONTEND_ROOT, 'src', 'vite-env.d.ts');
assert(existsSync(viteEnvDtsPath), 'src/vite-env.d.ts существует');

const viteEnvDtsContent = readFileSync(viteEnvDtsPath, 'utf-8');
assert(
  viteEnvDtsContent.includes('/// <reference types="vite/client" />'),
  'vite-env.d.ts содержит reference types="vite/client"',
);
assert(
  viteEnvDtsContent.includes('VITE_API_BASE_URL'),
  'vite-env.d.ts содержит VITE_API_BASE_URL',
);
assert(
  viteEnvDtsContent.includes('readonly'),
  'vite-env.d.ts содержит readonly модификатор',
);

// ─── shared/config/env.ts ────────────────────────────────────────────────────

console.log('\n── shared/config/env.ts ──');

const envTsPath = resolve(FRONTEND_ROOT, 'src', 'shared', 'config', 'env.ts');
assert(existsSync(envTsPath), 'src/shared/config/env.ts существует');

const envTsContent = readFileSync(envTsPath, 'utf-8');
assert(
  envTsContent.includes('VITE_API_BASE_URL'),
  'env.ts читает VITE_API_BASE_URL из import.meta.env',
);
assert(
  envTsContent.includes('export const apiBaseUrl'),
  'env.ts экспортирует apiBaseUrl',
);
assert(
  envTsContent.includes('validateApiBaseUrl'),
  'env.ts экспортирует validateApiBaseUrl (прямо или ре-экспортом)',
);
assert(
  !envTsContent.includes('console.log') && !envTsContent.includes('console.error'),
  'env.ts не логирует секреты',
);

// ─── Итог ────────────────────────────────────────────────────────────────────

console.log(`\nИтого: ${passed} пройдено, ${failed} провалено из ${passed + failed}`);

if (failed > 0) {
  process.exit(1);
}

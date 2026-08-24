/**
 * Регрессионная проверка подсветки ячейки «Освоено %».
 *
 * Сценарий бага: rawPercent = 80.4 → roundMoney → 80,
 * статус «Риск» (isRisk = true), но подсветка сравнивала округлённое 80 > 80 = false
 * и не применяла жёлтый фон.
 *
 * Запуск:  npx tsx scripts/test-report-highlight.ts
 */

import { getPercentCellHighlight } from '../src/widgets/reports/reportCellHighlight';

// ─── Тестовые сценарии ───────────────────────────────────────────────────────

interface Case {
  name: string;
  data: { isOverspent: boolean; isRisk: boolean };
  expectedCss: string;
  expectedBg: string | null;
}

const cases: Case[] = [
  {
    name: 'rawPercent = 80.4 → округляется до 80%, isRisk = true → жёлтый фон',
    data: { isOverspent: false, isRisk: true },
    expectedCss: 'text-end text-warning fw-bold',
    expectedBg: '#fff3cd',
  },
  {
    name: 'rawPercent = 105.2 → isOverspent = true → красный фон',
    data: { isOverspent: true, isRisk: true },
    expectedCss: 'text-end text-danger fw-bold',
    expectedBg: '#f8d7da',
  },
  {
    name: 'rawPercent = 50 → ни риск, ни перерасход → без фона',
    data: { isOverspent: false, isRisk: false },
    expectedCss: 'text-end',
    expectedBg: null,
  },
  {
    name: 'rawPercent = 80.0 → isRisk = false (не строго >80) → без фона',
    data: { isOverspent: false, isRisk: false },
    expectedCss: 'text-end',
    expectedBg: null,
  },
  {
    name: 'rawPercent = 100.0 → isOverspent = false (не строго >100) → без фона',
    data: { isOverspent: false, isRisk: true },
    expectedCss: 'text-end text-warning fw-bold',
    expectedBg: '#fff3cd',
  },
  {
    name: 'null data (строка итогов) → без фона',
    data: null as unknown as { isOverspent: boolean; isRisk: boolean },
    expectedCss: 'text-end',
    expectedBg: null,
  },
];

// ─── Прогон ──────────────────────────────────────────────────────────────────

let passed = 0;
let failed = 0;

for (const c of cases) {
  const result = getPercentCellHighlight(c.data);
  const cssOk = result.cssClass === c.expectedCss;
  const bgOk = result.backgroundColor === c.expectedBg;

  if (cssOk && bgOk) {
    console.log(`  PASS  ${c.name}`);
    passed++;
  } else {
    console.error(`  FAIL  ${c.name}`);
    if (!cssOk) console.error(`        cssClass: expected "${c.expectedCss}", got "${result.cssClass}"`);
    if (!bgOk) console.error(`        backgroundColor: expected ${c.expectedBg}, got ${result.backgroundColor}`);
    failed++;
  }
}

console.log(`\nИтого: ${passed} пройдено, ${failed} провалено из ${cases.length}`);

if (failed > 0) {
  process.exit(1);
}

/**
 * Регрессионные тесты для filter-query.ts
 *
 * Запуск:  npx tsx scripts/test-filter-query.ts
 */

import {
  parseTimesheetQuery,
  parseReportQuery,
  serializeTimesheetQuery,
  serializeReportQuery,
  validateKnownId,
  replaceKnownParams,
  TIMESHEET_QUERY_KEYS,
  REPORT_QUERY_KEYS,
  type FilterDefaults,
  type TimesheetQuery,
  type ReportQuery,
} from '../src/shared/lib/filter-query';

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

function assertDeepEqual<T>(actual: T, expected: T, name: string): void {
  const a = JSON.stringify(actual);
  const e = JSON.stringify(expected);
  assert(a === e, name, a === e ? undefined : `expected ${e}, got ${a}`);
}

function paramsToString(params: URLSearchParams): string {
  return params.toString();
}

// ─── Тестовые данные ─────────────────────────────────────────────────────────

const DEFAULTS: FilterDefaults = { year: 2026, month: 3 };
const KNOWN_EMPLOYEES = new Set(['emp-1', 'emp-2']);
const KNOWN_PROJECTS = new Set(['proj-1', 'proj-2']);

// ─── parseTimesheetQuery ─────────────────────────────────────────────────────

console.log('\n── parseTimesheetQuery ──');

// Missing params → defaults
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams(), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'missing params → defaults',
);

// Valid params
assertDeepEqual(
  parseTimesheetQuery(
    new URLSearchParams('year=2025&month=6&employeeId=emp-1&projectId=proj-2&page=3'),
    DEFAULTS,
    KNOWN_EMPLOYEES,
    KNOWN_PROJECTS,
  ),
  { year: 2025, month: 6, employeeId: 'emp-1', projectId: 'proj-2', page: 3 },
  'valid params parsed correctly',
);

// Explicit defaults → same result as missing
assertDeepEqual(
  parseTimesheetQuery(
    new URLSearchParams('year=2026&month=3&page=1'),
    DEFAULTS,
    KNOWN_EMPLOYEES,
    KNOWN_PROJECTS,
  ),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'explicit defaults → same as missing',
);

// Invalid year: "12abc"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('year=12abc'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid year "12abc" → default year',
);

// Invalid year: "1.5"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('year=1.5'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid year "1.5" → default year',
);

// Invalid year: "NaN"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('year=NaN'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid year "NaN" → default year',
);

// Invalid year: "Infinity"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('year=Infinity'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid year "Infinity" → default year',
);

// Invalid year: "0"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('year=0'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid year "0" → default year',
);

// Invalid year: "-1"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('year=-1'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid year "-1" → default year',
);

// Future year → clamped
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('year=9999'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'future year 9999 → clamped to 2026',
);

// Invalid month: "0"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('month=0'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid month "0" → default month',
);

// Invalid month: "13"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('month=13'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid month "13" → default month',
);

// Invalid month: "-1"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('month=-1'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid month "-1" → default month',
);

// Invalid month: "1.5"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('month=1.5'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid month "1.5" → default month',
);

// Invalid page: "0"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('page=0'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid page "0" → 1',
);

// Invalid page: "-1"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('page=-1'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid page "-1" → 1',
);

// Invalid page: "abc"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('page=abc'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid page "abc" → 1',
);

// Invalid page: "1.5"
assertDeepEqual(
  parseTimesheetQuery(new URLSearchParams('page=1.5'), DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'invalid page "1.5" → 1',
);

// Unknown employeeId → ''
assertDeepEqual(
  parseTimesheetQuery(
    new URLSearchParams('employeeId=unknown-emp'),
    DEFAULTS,
    KNOWN_EMPLOYEES,
    KNOWN_PROJECTS,
  ),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'unknown employeeId → empty',
);

// Unknown projectId → ''
assertDeepEqual(
  parseTimesheetQuery(
    new URLSearchParams('projectId=unknown-proj'),
    DEFAULTS,
    KNOWN_EMPLOYEES,
    KNOWN_PROJECTS,
  ),
  { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 },
  'unknown projectId → empty',
);

// Known employeeId + unknown projectId
assertDeepEqual(
  parseTimesheetQuery(
    new URLSearchParams('employeeId=emp-1&projectId=unknown-proj'),
    DEFAULTS,
    KNOWN_EMPLOYEES,
    KNOWN_PROJECTS,
  ),
  { year: 2026, month: 3, employeeId: 'emp-1', projectId: '', page: 1 },
  'known employeeId + unknown projectId',
);

// Parser does not mutate input
{
  const params = new URLSearchParams('year=2025&month=6');
  const before = params.toString();
  parseTimesheetQuery(params, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  assert(params.toString() === before, 'parser does not mutate input');
}

// ─── parseReportQuery ────────────────────────────────────────────────────────

console.log('\n── parseReportQuery ──');

// Missing → defaults
assertDeepEqual(
  parseReportQuery(new URLSearchParams(), DEFAULTS),
  { year: 2026, month: 3 },
  'missing params → defaults',
);

// Valid
assertDeepEqual(
  parseReportQuery(new URLSearchParams('year=2025&month=12'), DEFAULTS),
  { year: 2025, month: 12 },
  'valid year=2025 month=12',
);

// Future year → clamped
assertDeepEqual(
  parseReportQuery(new URLSearchParams('year=3000'), DEFAULTS),
  { year: 2026, month: 3 },
  'future year → clamped',
);

// Invalid month → default
assertDeepEqual(
  parseReportQuery(new URLSearchParams('month=0'), DEFAULTS),
  { year: 2026, month: 3 },
  'invalid month "0" → default',
);

// ─── serializeTimesheetQuery ─────────────────────────────────────────────────

console.log('\n── serializeTimesheetQuery ──');

// All defaults → empty string
{
  const q: TimesheetQuery = { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 };
  const s = serializeTimesheetQuery(q, DEFAULTS);
  assert(paramsToString(s) === '', 'all defaults → empty query string');
}

// Non-default year
{
  const q: TimesheetQuery = { year: 2025, month: 3, employeeId: '', projectId: '', page: 1 };
  const s = serializeTimesheetQuery(q, DEFAULTS);
  assert(paramsToString(s) === 'year=2025', 'non-default year only');
}

// Non-default month
{
  const q: TimesheetQuery = { year: 2026, month: 6, employeeId: '', projectId: '', page: 1 };
  const s = serializeTimesheetQuery(q, DEFAULTS);
  assert(paramsToString(s) === 'month=6', 'non-default month only');
}

// Non-default page
{
  const q: TimesheetQuery = { year: 2026, month: 3, employeeId: '', projectId: '', page: 5 };
  const s = serializeTimesheetQuery(q, DEFAULTS);
  assert(paramsToString(s) === 'page=5', 'non-default page only');
}

// All non-default
{
  const q: TimesheetQuery = { year: 2025, month: 6, employeeId: 'emp-1', projectId: 'proj-2', page: 3 };
  const s = serializeTimesheetQuery(q, DEFAULTS);
  const str = paramsToString(s);
  assert(str.includes('year=2025'), 'all non-default: contains year');
  assert(str.includes('month=6'), 'all non-default: contains month');
  assert(str.includes('employeeId=emp-1'), 'all non-default: contains employeeId');
  assert(str.includes('projectId=proj-2'), 'all non-default: contains projectId');
  assert(str.includes('page=3'), 'all non-default: contains page');
}

// Only known keys in output
{
  const q: TimesheetQuery = { year: 2025, month: 6, employeeId: 'emp-1', projectId: 'proj-2', page: 3 };
  const s = serializeTimesheetQuery(q, DEFAULTS);
  const keys = [...s.keys()];
  const allKnown = keys.every((k) => (TIMESHEET_QUERY_KEYS as readonly string[]).includes(k));
  assert(allKnown, 'serialized output contains only known keys');
}

// ─── serializeReportQuery ────────────────────────────────────────────────────

console.log('\n── serializeReportQuery ──');

// All defaults → empty
{
  const q: ReportQuery = { year: 2026, month: 3 };
  const s = serializeReportQuery(q, DEFAULTS);
  assert(paramsToString(s) === '', 'all defaults → empty');
}

// Non-default
{
  const q: ReportQuery = { year: 2025, month: 12 };
  const s = serializeReportQuery(q, DEFAULTS);
  const str = paramsToString(s);
  assert(str.includes('year=2025'), 'non-default year');
  assert(str.includes('month=12'), 'non-default month');
}

// ─── validateKnownId ─────────────────────────────────────────────────────────

console.log('\n── validateKnownId ──');

assert(validateKnownId('emp-1', KNOWN_EMPLOYEES) === 'emp-1', 'known ID returned as-is');
assert(validateKnownId('unknown', KNOWN_EMPLOYEES) === '', 'unknown ID → empty');
assert(validateKnownId('', KNOWN_EMPLOYEES) === '', 'empty ID → empty');

// ─── replaceKnownParams ──────────────────────────────────────────────────────

console.log('\n── replaceKnownParams ──');

// Preserves unknown keys
{
  const existing = new URLSearchParams('year=2025&foo=bar&baz=qux');
  const serialized = new URLSearchParams('year=2026&month=6');
  const result = replaceKnownParams(existing, TIMESHEET_QUERY_KEYS, serialized);
  assert(result.get('year') === '2026', 'replaceKnownParams: year replaced');
  assert(result.get('month') === '6', 'replaceKnownParams: month added');
  assert(result.get('foo') === 'bar', 'replaceKnownParams: unknown key "foo" preserved');
  assert(result.get('baz') === 'qux', 'replaceKnownParams: unknown key "baz" preserved');
}

// Removes known keys not in serialized
{
  const existing = new URLSearchParams('year=2025&month=6&page=3&foo=bar');
  const serialized = new URLSearchParams('year=2026'); // compact: month/page omitted
  const result = replaceKnownParams(existing, TIMESHEET_QUERY_KEYS, serialized);
  assert(result.get('year') === '2026', 'replaceKnownParams: year replaced');
  assert(result.get('month') === null, 'replaceKnownParams: old month removed');
  assert(result.get('page') === null, 'replaceKnownParams: old page removed');
  assert(result.get('foo') === 'bar', 'replaceKnownParams: unknown key preserved');
}

// Does not mutate input
{
  const existing = new URLSearchParams('year=2025&foo=bar');
  const serialized = new URLSearchParams('year=2026');
  const existingBefore = existing.toString();
  replaceKnownParams(existing, TIMESHEET_QUERY_KEYS, serialized);
  assert(existing.toString() === existingBefore, 'replaceKnownParams does not mutate existing');
}

// Report keys
{
  const existing = new URLSearchParams('year=2025&extra=data');
  const serialized = new URLSearchParams('month=12');
  const result = replaceKnownParams(existing, REPORT_QUERY_KEYS, serialized);
  assert(result.get('year') === null, 'replaceKnownParams (report): old year removed');
  assert(result.get('month') === '12', 'replaceKnownParams (report): month added');
  assert(result.get('extra') === 'data', 'replaceKnownParams (report): unknown key preserved');
}

// ─── Roundtrip: parse → serialize → parse ────────────────────────────────────

console.log('\n── Roundtrip ──');

{
  const original: TimesheetQuery = { year: 2025, month: 6, employeeId: 'emp-1', projectId: 'proj-2', page: 3 };
  const serialized = serializeTimesheetQuery(original, DEFAULTS);
  const parsed = parseTimesheetQuery(serialized, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  assertDeepEqual(parsed, original, 'roundtrip timesheet: parse(serialize(q)) === q');
}

{
  const original: ReportQuery = { year: 2025, month: 12 };
  const serialized = serializeReportQuery(original, DEFAULTS);
  const parsed = parseReportQuery(serialized, DEFAULTS);
  assertDeepEqual(parsed, original, 'roundtrip report: parse(serialize(q)) === q');
}

// Defaults roundtrip: serialize defaults → empty → parse → defaults
{
  const q: TimesheetQuery = { year: 2026, month: 3, employeeId: '', projectId: '', page: 1 };
  const serialized = serializeTimesheetQuery(q, DEFAULTS);
  const parsed = parseTimesheetQuery(serialized, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  assertDeepEqual(parsed, q, 'defaults roundtrip: serialize → empty → parse → defaults');
}

// ─── Canonical URL normalization ─────────────────────────────────────────────

console.log('\n── Canonical URL normalization ──');

// Invalid params should canonicalize to valid form
{
  const invalid = new URLSearchParams('year=12abc&month=13&employeeId=unknown&projectId=unknown&page=0');
  const parsed = parseTimesheetQuery(invalid, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  const canonical = serializeTimesheetQuery(parsed, DEFAULTS);
  const merged = replaceKnownParams(invalid, TIMESHEET_QUERY_KEYS, canonical);

  // After canonicalization, all known params should be valid
  const reparsed = parseTimesheetQuery(merged, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  assertDeepEqual(reparsed, parsed, 'canonical: invalid params normalize to valid state');

  // Unknown keys should be preserved (none in this case)
  assert(merged.get('unknownKey') === null, 'canonical: no unknown keys in this test');
}

// Invalid params with unknown keys preserved
{
  const invalid = new URLSearchParams('year=12abc&month=13&foo=bar&baz=qux');
  const parsed = parseTimesheetQuery(invalid, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  const canonical = serializeTimesheetQuery(parsed, DEFAULTS);
  const merged = replaceKnownParams(invalid, TIMESHEET_QUERY_KEYS, canonical);

  // Unknown keys preserved
  assert(merged.get('foo') === 'bar', 'canonical: unknown key "foo" preserved');
  assert(merged.get('baz') === 'qux', 'canonical: unknown key "baz" preserved');

  // Known params normalized
  const reparsed = parseTimesheetQuery(merged, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  assertDeepEqual(reparsed, parsed, 'canonical: known params normalized with unknown keys preserved');
}

// Valid params should not change after canonicalization
{
  const valid = new URLSearchParams('year=2025&month=6&employeeId=emp-1&projectId=proj-2&page=3');
  const parsed = parseTimesheetQuery(valid, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  const canonical = serializeTimesheetQuery(parsed, DEFAULTS);
  const merged = replaceKnownParams(valid, TIMESHEET_QUERY_KEYS, canonical);

  // Should be identical
  assert(merged.toString() === valid.toString(), 'canonical: valid params unchanged');
}

// Defaults should canonicalize to empty query string
{
  const defaults = new URLSearchParams('year=2026&month=3&page=1');
  const parsed = parseTimesheetQuery(defaults, DEFAULTS, KNOWN_EMPLOYEES, KNOWN_PROJECTS);
  const canonical = serializeTimesheetQuery(parsed, DEFAULTS);
  const merged = replaceKnownParams(defaults, TIMESHEET_QUERY_KEYS, canonical);

  // Should be empty (all defaults)
  assert(merged.toString() === '', 'canonical: defaults → empty query string');
}

// Report canonical: invalid params normalize
{
  const invalid = new URLSearchParams('year=12abc&month=13');
  const parsed = parseReportQuery(invalid, DEFAULTS);
  const canonical = serializeReportQuery(parsed, DEFAULTS);
  const merged = replaceKnownParams(invalid, REPORT_QUERY_KEYS, canonical);

  // Should be empty (defaults)
  assert(merged.toString() === '', 'canonical report: invalid → defaults → empty');
}

// Report canonical: unknown keys preserved
{
  const invalid = new URLSearchParams('year=12abc&extra=data');
  const parsed = parseReportQuery(invalid, DEFAULTS);
  const canonical = serializeReportQuery(parsed, DEFAULTS);
  const merged = replaceKnownParams(invalid, REPORT_QUERY_KEYS, canonical);

  assert(merged.get('extra') === 'data', 'canonical report: unknown key preserved');
  assert(merged.toString() === 'extra=data', 'canonical report: only unknown key remains');
}

// ─── Итог ────────────────────────────────────────────────────────────────────

console.log(`\nИтого: ${passed} пройдено, ${failed} провалено из ${passed + failed}`);

if (failed > 0) {
  process.exit(1);
}
